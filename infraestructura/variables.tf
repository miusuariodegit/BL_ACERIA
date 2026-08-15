# [GPX-DOC-v1] ==========================================================================================
# Variables de entrada del modulo Terraform: parametrizan region, nombre y tamano del cluster EKS,
# tipo de instancia de los nodos y acceso SSH. Los valores reales se pasan con -var en terraform apply
# (ver infraestructura/Readme.md).
# ==============================================================================================

variable "region" {
  description = "AWS region"
  type        = string
  default     = "us-east-1"
}

variable "cluster_name" {
  description = "EKS cluster name"
  type        = string
  default     = "eks-aceria-east1"
}

variable "kubernetes_version" {
  description = "Kubernetes version"
  type        = string
  default     = "1.36"
}

variable "node_instance_type" {
  description = "EC2 instance type for worker nodes"
  type        = string
  default     = "t3.medium"
}

variable "desired_nodes" {
  description = "Desired number of worker nodes"
  type        = number
  default     = 2
}

variable "min_nodes" {
  description = "Minimum number of worker nodes"
  type        = number
  default     = 1
}

variable "max_nodes" {
  description = "Maximum number of worker nodes"
  type        = number
  default     = 3
}

variable "ssh_key_name" {
  description = "EC2 key pair name for SSH access to worker nodes"
  type        = string
  default     = ""
}

variable "ssh_cidr" {
  description = "CIDR block allowed to SSH into worker nodes"
  type        = string
  default     = "0.0.0.0/0"
}
