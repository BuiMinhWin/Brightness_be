resource "aws_db_subnet_group" "this" {
  name       = "${local.name_prefix}-db"
  subnet_ids = module.vpc.private_subnets

  tags = local.common_tags
}

resource "aws_db_instance" "this" {
  identifier              = "${local.name_prefix}-pgsql"
  engine                  = "postgres"
  engine_version          = var.db_engine_version
  instance_class          = var.db_instance_class
  allocated_storage       = var.db_allocated_storage
  username                = var.db_master_username
  password                = var.db_master_password
  db_name                 = var.database_name
  port                    = 5432
  storage_encrypted       = true
  skip_final_snapshot     = true
  backup_retention_period = 7

  vpc_security_group_ids = [aws_security_group.rds.id]
  db_subnet_group_name   = aws_db_subnet_group.this.name

  publicly_accessible = false
  deletion_protection = false

  tags = local.common_tags
}

resource "aws_ssm_parameter" "db_connection_string" {
  name  = "/${local.name_prefix}/db/connection-string"
  type  = "SecureString"
  value = "Host=${aws_db_instance.this.address};Port=5432;Database=${var.database_name};Username=${var.db_master_username};Password=${var.db_master_password};IncludeErrorDetail=false"

  tags = local.common_tags
}
