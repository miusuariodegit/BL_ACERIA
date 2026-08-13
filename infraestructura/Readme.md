Para realizar despliegues ejecute:

terraform init

Obtener la ip publica para crear el clúster

terraform apply -var="ssh_key_name=ssh_key_1" -var="ssh_cidr=187.243.216.3/32"

Al iniciar el clúster es necesario aplicar estos dos comandos 

aws eks update-kubeconfig --region us-east-1 --name eks-aceria-east1

kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.12.2/deploy/static/provider/aws/deploy.yaml


Aqui ejecutar el deploy mediante git hub ejecutando los siguientes pasos:

git add .

git tag -a v1.0.14 -m “release versión 1.0.14”

git push origin v1.0.14


para borrar el cluster 

kubectl delete -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.12.2/deploy/static/provider/aws/deploy.yaml

terraform destroy



