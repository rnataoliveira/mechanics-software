variable "cluster_name" {
  description = "EKS cluster name"
  type        = string
  default     = "mechanics-software"
}

variable "aws_region" {
  description = "AWS region"
  type        = string
  default     = "us-east-1"
}
