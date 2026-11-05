# Kubernetes

Os manifests Kubernetes deste serviço são mantidos no repositório
[`FCG.Orchestration`](https://github.com/BrnMatos87/FCG.Orchestration), que é a
fonte única da implantação no AKS.

- [Manifests do Catalog](https://github.com/BrnMatos87/FCG.Orchestration/tree/main/k8s/catalog)
- [Kustomize da solução](https://github.com/BrnMatos87/FCG.Orchestration/tree/main)

Configurações compartilhadas de MongoDB, Redis, Elasticsearch e RabbitMQ
também pertencem ao `FCG.Orchestration`. Não adicione Secrets ou credenciais
neste repositório. Alterações de infraestrutura devem seguir o fluxo `fase4`
para `main` no repositório de orquestração.
