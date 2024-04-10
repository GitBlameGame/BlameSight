data "aws_secretsmanager_secret_version" "discord_bot_token" {
  secret_id = aws_secretsmanager_secret.discord_bot_token.arn
}

resource "aws_secretsmanager_secret" "discord_bot_token" {
  name = "discord-bot-token-v2"
}

resource "aws_s3_bucket" "beanstalk_bucket_bot" {
  bucket        = "${local.account-id}-deploy-bucket-bot"
  force_destroy = true
}

resource "aws_elastic_beanstalk_application" "bot_app" {
  name        = "bot-app"
  description = "App for C# Discord bot"
}

resource "aws_elastic_beanstalk_environment" "bot_env" {
  name                = "bot-env"
  application         = aws_elastic_beanstalk_application.bot_app.name
  solution_stack_name = "64bit Windows Server 2022 v2.14.1 running IIS 10.0"
  tier                = "WebServer"

  setting {
    namespace = "aws:ec2:vpc"
    name      = "VPCId"
    value     = module.vpc.vpc_id
  }
  setting {
    namespace = "aws:autoscaling:launchconfiguration"
    name      = "IamInstanceProfile"
    value     = aws_iam_instance_profile.beanstalk_ec2.name
  }
  setting {
    namespace = "aws:ec2:vpc"
    name      = "Subnets"
    value     = join(",", module.vpc.public_subnets)
  }
  setting {
    namespace = "aws:ec2:instances"
    name      = "InstanceTypes"
    value     = "t3.micro"
  }
  setting {
    namespace = "aws:elasticbeanstalk:healthreporting:system"
    name      = "SystemType"
    value     = "basic"
  }
  setting {
    namespace = "aws:elasticbeanstalk:application"
    name      = "Application Healthcheck URL"
    value     = "/"
  }
  setting {
    namespace = "aws:elasticbeanstalk:command"
    name      = "Timeout"
    value     = "60"
  }
  setting {
    namespace = "aws:elasticbeanstalk:command"
    name      = "IgnoreHealthCheck"
    value     = "true"
  }
  setting {
    namespace = "aws:elasticbeanstalk:environment"
    name      = "EnvironmentType"
    value     = "SingleInstance"
  }
  setting {
    namespace = "aws:elasticbeanstalk:managedactions"
    name      = "ManagedActionsEnabled"
    value     = "false"
  }
  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "DISCORD_BOT_TOKEN"
    value     = data.aws_secretsmanager_secret_version.discord_bot_token.secret_string
  }
  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "API_ENDPOINT"
    value     = aws_elastic_beanstalk_environment.api_env.endpoint_url
  }
}

