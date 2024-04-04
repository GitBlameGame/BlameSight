module "vpc" {
  source  = "terraform-aws-modules/vpc/aws"
  version = "5.5.3"

  name = "main-vpc"
  cidr = "192.168.0.0/16"

  azs              = ["eu-west-1a", "eu-west-1b"]
  private_subnets  = ["192.168.1.0/24", "192.168.2.0/24"]
  public_subnets   = ["192.168.101.0/24", "192.168.102.0/24"]
  database_subnets = ["192.168.201.0/24", "192.168.202.0/24"]

  create_database_subnet_group           = true
  create_database_subnet_route_table     = true
  create_database_internet_gateway_route = true
  database_subnet_group_name             = "db-subnet-group"

  enable_dns_hostnames = true
  enable_dns_support   = true
}

resource "aws_security_group" "rds" {
  name_prefix = "rds-"

  vpc_id = module.vpc.vpc_id
  ingress {
    from_port   = "5432"
    to_port     = "5432"
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }
}