resource "aws_cloudwatch_log_group" "services" {
  for_each = local.services

  name              = "/ecs/${local.name_prefix}-${each.key}"
  retention_in_days = 30

  tags = local.common_tags
}
