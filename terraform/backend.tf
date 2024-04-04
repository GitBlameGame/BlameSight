terraform {
  backend "s3" {
    bucket         = "263883060207-state"
    key            = "state/terraform.tfstate"
    region         = "eu-west-1"
    dynamodb_table = "263883060207-state"
  }
}
