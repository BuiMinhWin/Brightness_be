locals {
  name_prefix = "${var.project}-${var.environment}"

  service_discovery_domain = "${local.name_prefix}.local"

  services = {
    gateway = {
      container_name = "gateway-api"
      port           = 8088
      cpu            = 1024
      memory         = 2048
      desired_count  = 2
      public         = true
      health_check_path = "/health"
    }
    authentication = {
      container_name = "authentication-api"
      port           = 5000
      cpu            = 512
      memory         = 1024
      desired_count  = 2
      public         = false
      health_check_path = "/health"
    }
    measurement = {
      container_name = "measurement-api"
      port           = 8080
      cpu            = 512
      memory         = 1024
      desired_count  = 1
      public         = false
      health_check_path = "/health"
    }
    challenge = {
      container_name = "challenge-api"
      port           = 8083
      cpu            = 512
      memory         = 1024
      desired_count  = 1
      public         = false
      health_check_path = "/health"
    }
    social = {
      container_name = "social-api"
      port           = 8084
      cpu            = 512
      memory         = 1024
      desired_count  = 1
      public         = false
      health_check_path = "/health"
    }
    billing = {
      container_name = "billing-api"
      port           = 8085
      cpu            = 512
      memory         = 1024
      desired_count  = 1
      public         = false
      health_check_path = "/health"
    }
    media = {
      container_name = "media-api"
      port           = 8086
      cpu            = 512
      memory         = 1024
      desired_count  = 1
      public         = false
      health_check_path = "/health"
    }
    chat = {
      container_name = "chat-service"
      port           = 8087
      cpu            = 512
      memory         = 1024
      desired_count  = 1
      public         = false
      health_check_path = "/health"
    }
  }

  common_tags = merge({
    Project     = var.project
    Environment = var.environment
  }, var.extra_tags)
}
