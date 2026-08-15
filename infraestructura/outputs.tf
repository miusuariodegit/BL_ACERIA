# [GPX-DOC-v1] ==========================================================================================
# Salidas del modulo Terraform: datos del cluster EKS (nombre, endpoint, version, certificado),
# el rol IAM del driver CSI de EBS y las IPs de nodos/API, utiles para configurar kubectl y para
# diagnostico de red tras el aprovisionamiento.
# ==============================================================================================

output "cluster_name" {
  value = aws_eks_cluster.this.name
}

output "cluster_endpoint" {
  value = aws_eks_cluster.this.endpoint
}

output "cluster_version" {
  value = aws_eks_cluster.this.version
}

output "cluster_certificate_authority" {
  value     = aws_eks_cluster.this.certificate_authority[0].data
  sensitive = true
}

output "ebs_csi_role_arn" {
  value = aws_iam_role.ebs_csi.arn
}

output "node_public_ips" {
  value = data.aws_instances.nodes.public_ips
}

output "node_private_ips" {
  value = data.aws_instances.nodes.private_ips
}

output "cluster_api_ips" {
  value = data.dns_a_record_set.cluster_api.addrs
}



