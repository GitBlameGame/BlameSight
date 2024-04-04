module "rds" {
  source  = "terraform-aws-modules/rds/aws"
  version = "6.5.2"

  identifier = "blamesightdb"

  family               = "postgres16"
  major_engine_version = "16"
  engine               = "postgres"
  engine_version       = "16"

  instance_class      = "db.t3.micro"
  create_db_instance  = true
  allocated_storage   = 10
  deletion_protection = false
  skip_final_snapshot = true

  db_subnet_group_name   = module.vpc.database_subnet_group_name
  vpc_security_group_ids = [aws_security_group.rds.id]
  publicly_accessible    = true

  db_name                     = local.db-name
  username                    = "dbadmin"
  port                        = "5432"
  manage_master_user_password = true
}