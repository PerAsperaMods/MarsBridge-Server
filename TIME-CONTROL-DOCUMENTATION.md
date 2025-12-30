# Per Aspera Time Control System - Documentation

## 🎯 **Système de Contrôle Temporel - MarsBridge**

### 📋 **Vue d'ensemble**
Le système MarsBridge permet de contrôler la vitesse temporelle du jeu Per Aspera via une API REST et SignalR. Le contrôle principal s'effectue via `Universe.gameSpeed`.

---

## 🔍 **Découvertes Techniques**

### 🎮 **Mécanisme de Vitesse du Jeu**
- **Contrôle primaire** : `Universe.gameSpeed` (float)
- **Contrôle secondaire** : `SliceMaster.tickTimeMultiplier` (fallback)
- **Contrôle tertiaire** : `SliceMaster.tickTime` (dernier recours)
- **Échelle temporelle** : 1x = 1 sol martien par seconde

### 🌌 **Durée d'un Sol Martien**
- **Réalité** : 24h 37min 22s terrestres
- **Jeu** : Simplifié à 24h terrestres (TICKS_PER_DAY = 1)
- **Vitesse normale** : 1 sol/seconde

### 📊 **Constantes du Jeu**
```csharp
public const int TICKS_PER_DAY = 1;  // 1 tick = 1 jour
```

---

## 🧮 **Calculs des Multipliers**

### 📈 **Formule Générale**
```
Multiplier = Temps_désiré / Temps_actuel
```

### 🎯 **Multipliers Courants**
| Multiplier | Effet | Description |
|------------|-------|-------------|
| 10.0x | Ultra-rapide | 10 sols/seconde |
| 5.0x | Très rapide | 5 sols/seconde |
| 1.0x | Normal | 1 sol/seconde |
| 0.016x | Jeu normal | 1 sol/minute |
| 0.01x | Lent | 1 sol/1.5min |
| 0.000277x | Réaliste | 1 sol/heure |
| 0.0x | Pause | Jeu arrêté |

### 🔢 **Calculs Détaillés**
```javascript
// 1 sol par minute
1 / 60 = 0.016666... → 0.016x

// 1 sol par heure
1 / 3600 = 0.000277777... → 0.000277x

// 1 sol par jour terrestre
1 / 86400 = 0.000011574... → 0.000011x
```

---

## 🛠️ **Architecture Technique**

### 📡 **MarsBridge Server (ASP.NET Core 8.0)**
- **Endpoint** : `POST /api/climate/ticktime`
- **Validation** : 0.0x - 10.0x (configurable)
- **SignalR** : Diffusion temps réel aux clients
- **Port** : 8080 (remote: 100.88.41.52:8080)

### 🎮 **MarsBridge Client (BepInEx 6)**
- **Plugin** : MarsBridgeClientPlugin.cs
- **Connexion** : SignalR Hub
- **Priorité** : Universe.gameSpeed → SliceMaster → Fallback
- **Logging** : Détaillé via LogAspera

### 🔄 **Flux de Données**
```
Web UI → MarsBridge Server → SignalR → BepInEx Client → Universe.gameSpeed
```

---

## 📝 **API Reference**

### 🚀 **Set TickTime**
```http
POST /api/climate/ticktime
Content-Type: application/json

{
  "multiplier": 0.016
}
```

**Réponses :**
- `200` : {"success":true, "message":"TickTime multiplier set to 0.016x successfully"}
- `400` : {"success":false, "message":"Invalid multiplier (must be 0.0-10.0)"}

### 📡 **Get TickTime**
```http
GET /api/climate/ticktime
```

**Réponse :**
```json
{
  "success": true,
  "message": "TickTime multiplier retrieved successfully",
  "data": 0.016
}
```

---

## 🎨 **Interface Utilisateur Web**

### 📱 **Fonctionnalités**
- [ ] Curseur pour contrôle continu (0.0x - 10.0x)
- [ ] Boutons prédéfinis (Pause, Normal, Rapide, Réaliste)
- [ ] Affichage du multiplier actuel
- [ ] Historique des changements
- [ ] Indicateur de connexion SignalR

### 🎯 **Préréglages Utiles**
```javascript
const presets = {
  pause: 0.0,        // Arrêt complet
  realistic: 0.000277, // 1 sol/heure
  normal: 0.016,     // 1 sol/minute
  fast: 1.0,         // 1 sol/seconde
  veryFast: 5.0      // 5 sols/seconde
};
```

---

## 🔧 **Configuration et Déploiement**

### ⚙️ **Variables d'environnement**
```bash
BAGET_API_KEY=your_api_key
MARSBRIDGE_PORT=8080
```

### 🚀 **Commandes de déploiement**
```bash
# Build et déploiement SDK
.\SDK\Build-SDK.ps1 -Deploy

# Redémarrage serveur
docker-compose restart marsbridge-server
```

### 📊 **Monitoring**
- **Logs serveur** : `/logs/marsbridge-.txt`
- **Logs client** : `BepInEx\LogOutput.log`
- **SignalR** : Connexion temps réel vérifiée

---

## 🎮 **Utilisation Pratique**

### 🕹️ **Contrôles Rapides**
```bash
# Via API directe
curl -X POST http://100.88.41.52:8080/api/climate/ticktime \
  -H "Content-Type: application/json" \
  -d '{"multiplier": 0.016}'

# Via PowerShell
Invoke-WebRequest -Uri "http://100.88.41.52:8080/api/climate/ticktime" \
  -Method POST -ContentType "application/json" \
  -Body '{"multiplier": 0.016}'
```

### 🎯 **Scénarios d'usage**
1. **Pause** (0.0x) : Pour planifier sans progression temporelle
2. **Normal** (0.016x) : Jeu standard avec progression visible
3. **Réaliste** (0.000277x) : Simulation longue durée
4. **Rapide** (5.0x) : Tests et développement accéléré

---

## 🔬 **Tests et Validation**

### ✅ **Tests Réalisés**
- [x] 0.0x : Pause complète
- [x] 0.01x : Ralenti 100x
- [x] 0.016x : 1 sol/minute
- [x] 0.000277x : 1 sol/heure
- [x] 5.0x : Accéléré 5x
- [x] SignalR : Communication temps réel
- [x] Logs : Traçabilité complète

### 🎯 **Points de Validation**
- API retourne succès pour tous les multipliers valides
- Client reçoit et applique les commandes
- Universe.gameSpeed est correctement modifié
- SignalR diffuse les changements
- Logs détaillés pour debugging

---

## 🚀 **Évolutions Futures**

### 🔮 **Améliorations Possibles**
- [ ] Interface web complète avec contrôles visuels
- [ ] Préréglages sauvegardés par utilisateur
- [ ] Intégration Discord/Twitch pour contrôles communautaires
- [ ] Statistiques de temps joué
- [ ] Mode automatique (accélération nocturne)
- [ ] Synchronisation multi-joueurs

### 📊 **Optimisations**
- [ ] Cache des valeurs fréquentes
- [ ] Validation côté client
- [ ] Limites configurables par utilisateur
- [ ] Historique des sessions

---

## 📚 **Références**

### 🔗 **Documentation Technique**
- [Per Aspera SDK Documentation](../SDK-Enhanced-Classes/)
- [BepInEx 6 Guide](../Internal_doc/bepinex6-docs/)
- [HarmonyX Patching](../Internal_doc/HarmonyX.wiki/)

### 🛠️ **Outils de Développement**
- [MarsBridge Server](../MarsBridge-Server/)
- [MarsBridge Client](../Individual-Mods/MarsBridge.Client/)
- [Scripts de déploiement](../scripts/)

---

*Documentation créée le 30 décembre 2025 - Système MarsBridge v1.0*</content>
<filePath">F:\ModPeraspera\MarsBridge-Server\TIME-CONTROL-DOCUMENTATION.md