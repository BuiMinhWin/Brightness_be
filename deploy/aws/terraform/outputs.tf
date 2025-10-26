output "alb_dns_name" {
  description = "DNS name of the public Application Load Balancer"
  value       = aws_lb.public.dns_name
}

output "service_discovery_namespace" {
  description = "Cloud Map namespace used for service discovery"
  value       = aws_service_discovery_private_dns_namespace.this.name
}

output "rds_endpoint" {
  description = "PostgreSQL endpoint"
  value       = aws_db_instance.this.address
}
