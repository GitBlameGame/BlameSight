resource "aws_s3_bucket" "newman_bucket" {
  bucket        = "${local.account-id}-newman"
  force_destroy = true
}