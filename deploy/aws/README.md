# Brightness backend AWS deployment

This folder contains Terraform and helper scripts to run the .NET microservices on AWS ECS Fargate with an RDS PostgreSQL backend. The workflow is:

1. Build and push container images for each service to Amazon ECR.
2. Provision networking, database, service discovery, load balancing, and ECS services with Terraform.
3. Update application configuration to resolve downstream services via AWS Cloud Map.
4. Redeploy when images or infrastructure change by updating the image tags and reapplying Terraform.

## Repo layout

- `terraform/` – infrastructure as code for VPC, RDS, ECS, ECR, Cloud Map, and ALB.
- `scripts/publish-images.ps1` – convenience script that builds and pushes all service images to ECR.

## Prerequisites

- AWS account and IAM user/role with permissions for VPC, EC2, IAM, ECS, ECR, RDS, CloudWatch Logs, Cloud Map, and SSM Parameter Store.
- AWS CLI v2 configured locally (`aws configure` or environment variables).
- Terraform >= 1.5.
- Docker engine to build images.
- (.NET builds happen inside the Dockerfiles, so the host machine does not require the .NET SDK.)

## Step 1: Decide deployment parameters

Update or override the Terraform variables (see `terraform/variables.tf`). At minimum you must supply:

- `aws_region` (defaults to ap-southeast-1).
- `db_master_password` via a `terraform.tfvars` file or the CLI (`-var`).
- `image_tags` map pointing to the tagged images you pushed (defaults to `latest`).

Example `deploy/aws/terraform/terraform.tfvars`:

```
aws_region       = "ap-southeast-1"
project          = "brightness"
environment      = "prod"
db_master_password = "change-me-strong"
image_tags = {
  gateway        = "2024-09-29"
  authentication = "2024-09-29"
}
```

Any service not listed in `image_tags` uses `default_image_tag`.

## Step 2: Build and publish container images

Sign in to ECR and push images. You can use the helper script:

```
powershell.exe -ExecutionPolicy Bypass -File deploy/aws/scripts/publish-images.ps1 `
  -AwsAccountId 123456789012 `
  -Region ap-southeast-1 `
  -RepositoryPrefix brightness-prod `
  -ImageTag 2024-09-29
```

That script:

- Creates (if necessary) `${RepositoryPrefix}-{service}` repositories.
- Builds each microservice with the Dockerfile that already ships in the repo.
- Tags and pushes the image as `${AwsAccountId}.dkr.ecr.${Region}.amazonaws.com/${RepositoryPrefix}-{service}:${ImageTag}`.

If you use a CI pipeline, replicate the same build matrix and push step using `aws ecr get-login-password` plus `docker build`/`docker push`.

## Step 3: Provision AWS infrastructure

Inside `deploy/aws/terraform` run:

```
terraform init
terraform plan
terraform apply
```

What Terraform creates:

- VPC with public and private subnets, NAT gateway, and routing.
- Security groups that isolate ALB, ECS tasks, and RDS.
- RDS PostgreSQL instance (private subnets) plus an SSM secure parameter containing the connection string used by the services.
- ECR repositories, ECS cluster, task/exec IAM roles, and CloudWatch log groups.
- AWS Cloud Map private DNS namespace so services can talk to each other by name (`http://authentication.<namespace>:5000`).
- Application Load Balancer that fronts the `gateway` service.

The apply output prints the ALB URL and Cloud Map namespace.

## Step 4: Configure the application for AWS

- `GatewayAPI/APIGateway.Presentation/Program.cs` now loads `ocelot.{EnvironmentName}.json`. Provide `ocelot.Production.json` that points to Cloud Map hostnames (for example `authentication.<namespace>`). The repository already includes a starter file to adjust.
- Ensure each downstream service exposes a health endpoint matching the ALB target group path (defaults to `/health` in Terraform). Modify `locals.services` if you need a different path.
- Secrets such as JWT signing keys should move to SSM Parameter Store or AWS Secrets Manager and be injected as additional `secrets` entries in `locals.services`.

## Step 5: Deploy updates

1. Build and push a new image tag.
2. Update `image_tags` (or `default_image_tag`) in Terraform variables.
3. Run `terraform apply`. Terraform registers a new task definition revision and performs a rolling deployment on ECS.

## Step 6: Operations

- Health and request logs stream to CloudWatch Logs groups named `/ecs/<project>-<env>-<service>`.
- RDS metrics and alarms can be added later with Terraform (`aws_cloudwatch_metric_alarm`).
- To roll back, point `image_tags` back to the previous tag and reapply.

## Clean up

To remove the stack: `terraform destroy`. This keeps ECR repositories by default (`prevent_destroy` on), so you must delete image repositories manually if you no longer need them.
