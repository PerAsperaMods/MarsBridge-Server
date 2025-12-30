# 🚀 Push vers GitHub Repository

## 📋 **Instructions de Déploiement**

Voici les commandes pour pousser le projet vers votre repository GitHub :

```bash
# 1. Se positionner dans le dossier MarsBridge-Server
cd "f:\ModPeraspera\MarsBridge-Server"

# 2. Initialiser le repository Git (si pas déjà fait)
git init

# 3. Ajouter le remote GitHub
git remote add origin https://github.com/PerAsperaMods/MarsBridge-Server.git

# 4. Ajouter tous les fichiers
git add .

# 5. Premier commit
git commit -m "🚀 Initial MarsBridge Server setup

- ASP.NET Core 8.0 server with REST API
- SignalR WebSocket Hub for real-time communication  
- RabbitMQ integration for message brokering
- Docker Compose setup with Portainer support
- Climate management service with history
- Multi-player support (Per Aspera + Satisfactory)
- Prometheus monitoring integration
- Complete production-ready configuration"

# 6. Push vers GitHub
git branch -M main
git push -u origin main
```

## 🎯 **Vérification du Push**

Après le push, vérifiez sur GitHub : https://github.com/PerAsperaMods/MarsBridge-Server

Vous devriez voir :
```
📁 MarsBridge-Server/
├── 📄 README.md                        # Documentation complète
├── 🐳 docker-compose.portainer.yml     # Configuration Docker
├── 🔐 .env.portainer                   # Variables environnement
├── 📁 src/                             # Code source ASP.NET Core
│   ├── 📄 Program.cs                   # Point d'entrée
│   ├── 📄 MarsBridge.Server.csproj     # Projet .NET
│   ├── 📄 appsettings.json             # Configuration
│   ├── 📄 Dockerfile                   # Image Docker
│   ├── 📁 Controllers/                 # API REST
│   ├── 📁 Hubs/                        # SignalR WebSockets
│   ├── 📁 Services/                    # Business Logic
│   └── 📁 Models/                      # Data Models
```

## 🚀 **Déploiement Portainer Suivant**

Une fois le code sur GitHub, dans Portainer :

1. **Stacks** → **Add stack**
2. **Repository** :
   - URL: `https://github.com/PerAsperaMods/MarsBridge-Server`
   - Reference: `refs/heads/main`
   - Compose path: `docker-compose.portainer.yml`
3. **Environment variables** (copier depuis .env.portainer)
4. **Deploy the stack**

## ✅ **Prêt pour Production !**

Le serveur MarsBridge sera accessible sur :
- 🌐 **API REST** : `http://your-server:8080`
- 🔌 **WebSocket** : `http://your-server:8081`  
- 🐰 **RabbitMQ** : `http://your-server:15672`
- 📊 **Grafana** : `http://your-server:3000`