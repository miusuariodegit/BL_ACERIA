para poder realizar despliegues

terraform init

obtener la ip publica para crear el cluster

terraform apply -var="ssh_key_name=ssh_key_1" -var="ssh_cidr=187.243.202.23/32"

al inicior el cluste es necesario aplicar estos dos comandos 

aws eks update-kubeconfig --region us-east-1 --name eks-aceria-east1
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.12.2/deploy/static/provider/aws/deploy.yaml


para aplicar kubernetes clonar el repositorio o navegar a la carpeta donde se tengan 
cd k8s

y aplicar los kubernetes

kubectl apply -f . 

para borrar el cluster 

kubectl delete -f . 
kubectl delete -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.12.2/deploy/static/provider/aws/deploy.yaml



