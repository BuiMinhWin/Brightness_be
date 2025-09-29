variable "aws_region" {
  description = "AWS region for all resources"
  type        = string
  default     = "ap-southeast-1"
}

variable "project" {
  description = "Project name used for tagging"
  type        = string
  default     = "brightness"
}

variable "environment" {
  description = "Deployment environment identifier"
  type        = string
  default     = "prod"
}

variable "vpc_cidr" {
  description = "CIDR block for the VPC"
  type        = string
  default     = "10.20.0.0/16"
}

variable "availability_zones" {
  description = "Availability zones to spread subnets across"
  type        = list(string)
  default     = ["ap-southeast-1a", "ap-southeast-1b"]
}

variable "public_subnets" {
  description = "CIDRs for the public subnets"
  type        = list(string)
  default     = ["10.20.0.0/24", "10.20.1.0/24"]
}

variable "private_subnets" {
  description = "CIDRs for the private subnets"
  type        = list(string)
  default     = ["10.20.10.0/24", "10.20.11.0/24"]
}

variable "db_instance_class" {
  description = "Instance class for the PostgreSQL RDS"
  type        = string
  default     = "db.t4g.micro"
}

variable "db_allocated_storage" {
  description = "Allocated storage in GB for the RDS instance"
  type        = number
  default     = 20
}

variable "db_engine_version" {
  description = "PostgreSQL engine version"
  type        = string
  default     = "16.4"
}

variable "db_master_username" {
  description = "Master username for RDS"
  type        = string
  default     = "brightness_admin"
}

variable "db_master_password" {
  description = "Master password for RDS"
  type        = string
  sensitive   = true
}

variable "database_name" {
  description = "Logical database name created in the RDS instance"
  type        = string
  default     = "brightnessdb"
}

variable "default_image_tag" {
  description = "Fallback container image tag"
  type        = string
  default     = "latest"
}

variable "image_tags" {
  description = "Optional override per service for container image tag"
  type        = map(string)
  default     = {}
}

variable "desired_count_overrides" {
  description = "Optional override per service for desired task count"
  type        = map(number)
  default     = {}
}

variable "alb_listener_port" {
  description = "External port exposed by the Application Load Balancer"
  type        = number
  default     = 80
}

variable "extra_tags" {
  description = "Additional tags to add to all resources"
  type        = map(string)
  default     = {}
}
