# MarsBridge Server - Distributed Climate Management System

🚀 **MarsBridge** est un serveur distribué qui connecte Per Aspera (terraforming Mars) avec Satisfactory (production industrielle) via un système de messagerie RabbitMQ et une API REST.

## 🌟 **Fonctionnalités**

- **API REST** pour données climatiques Mars en temps réel
- **WebSocket Hub** pour updates temps réel multi-joueurs
- **Message Broker RabbitMQ** pour communication asynchrone
- **Support Multi-Joueurs** Satisfactory simultanés
- **Monitoring Complet** avec Prometheus + Grafana
- **Docker Ready** avec Portainer.io support

## 🏗️ **Architecture**

```
Satisfactory Players (1-50+) → MarsBridge Server ← Per Aspera Game
                ↓                    ↓                    ↓
          WebSocket Client     [Docker Stack]       BepInX Plugin
          Resource Tracking         ↓              Climate Commands
                               RabbitMQ Broker
                               PostgreSQL DB
                               Redis Cache
                               Monitoring Stack
```

## 📋 **Pré-requis**

- Serveur avec Docker + Portainer.io installés
- Repository Git (GitHub/GitLab/Gitea)
- Accès admin Portainer

## 🐳 **Déploiement via Portainer + Git**

### 1️⃣ **Setup Repository Git**

```bash
# Créer le repository MarsBridge-Server
mkdir MarsBridge-Server
cd MarsBridge-Server

# Structure des fichiers
├── docker-compose.portainer.yml  # Configuration Portainer
├── .env.portainer                # Variables d'environnement
├── src/
│   ├── Dockerfile                # Image MarsBridge Server
│   └── MarsBridge.Server.csproj  # Projet ASP.NET Core (à créer)
├── config/
│   ├── nginx/                    # Configuration Nginx
│   ├── prometheus/               # Configuration Prometheus
│   └── grafana/                  # Dashboards Grafana
└── README.md                     # Ce fichier

# Commit et push vers votre Git
git init
git add .
git commit -m "Initial MarsBridge Server setup"
git remote add origin https://github.com/VotreUtilisateur/MarsBridge-Server
git push -u origin main
```

### 2️⃣ **Déploiement dans Portainer**

1. **Accéder à Portainer** : `https://votre-serveur:9443`

2. **Créer une Stack Git** :
   - Aller dans **Stacks** > **Add stack**
   - Choisir **Repository**
   - Renseigner :
     ```
     Repository URL: https://github.com/VotreUtilisateur/MarsBridge-Server
     Repository reference: refs/heads/main
     Compose path: docker-compose.portainer.yml
     ```

3. **Variables d'environnement** :
   ```bash
   # Copier le contenu de .env.portainer dans "Environment variables"
   POSTGRES_PASSWORD=YourSecurePostgresPassword2025!
   RABBITMQ_PASSWORD=YourSecureRabbitMQPassword2025!
   REDIS_PASSWORD=YourSecureRedisPassword2025!
   JWT_SECRET=YourVeryLongJWTSecretKeyForProductionUse!
   ```

4. **Déployer** : Cliquer sur **Deploy the stack**

### 3️⃣ **Vérification du Déploiement**

```bash
# Services disponibles après déploiement
✅ MarsBridge API        : http://votre-serveur:8080
✅ MarsBridge WebSocket  : http://votre-serveur:8081
✅ RabbitMQ Management  : http://votre-serveur:15672
✅ Grafana Dashboard    : http://votre-serveur:3000
✅ Prometheus Metrics   : http://votre-serveur:9090

# Health checks
curl http://votre-serveur:8080/health
curl http://votre-serveur:8080/api/climate/status
```

## 🔄 **Workflow de Développement**

### **Auto-Deploy depuis Git**

1. **Développement local** :
   ```bash
   # Modifier le code
   git add .
   git commit -m "Feature: nouveau endpoint climate"
   git push origin main
   ```

2. **Re-déploiement automatique** :
   - Dans Portainer > Stacks > MarsBridge
   - Cliquer **Pull and redeploy**
   - Ou configurer Webhook pour auto-deploy

### **Mise à jour via Portainer**

1. **Pull dernières modifications** :
   - Stack settings > Repository > **Pull latest changes**
   
2. **Re-build et redeploy** :
   - **Update the stack** après pull

## 🎯 **Avantages de cette Approche**

✅ **GitOps Workflow** - Code → Git → Auto-deploy  
✅ **Rollback facile** - Via Portainer UI ou Git revert  
✅ **Monitoring intégré** - Logs, metrics, health checks  
✅ **Secrets Management** - Variables d'environnement sécurisées  
✅ **Scaling horizontal** - Via Portainer stack scaling  

## 🚀 **Prochaines Étapes**

1. **Créer le projet ASP.NET Core** MarsBridge.Server
2. **Push vers votre Git repository**
3. **Déployer via Portainer**
4. **Connecter PerAspera Plugin** au server

## 🔧 **Troubleshooting**

- **Container fails** : Vérifier logs dans Portainer > Containers
- **Build errors** : Vérifier Dockerfile et dependencies
- **Network issues** : Vérifier ports et firewall serveur
- **Git sync** : Vérifier repository access et credentials