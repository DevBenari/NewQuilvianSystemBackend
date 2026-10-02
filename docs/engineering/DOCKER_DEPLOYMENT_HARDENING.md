# Docker Deployment Hardening

## Overview

Dokumen ini mencatat peningkatan keamanan deployment dan maintenance Docker untuk Quilvian V2 Developer Environment.

Tujuan:

- mencegah penumpukan image Docker
- mencegah pertumbuhan log container tanpa batas
- menjaga stabilitas VM Docker
- mengurangi risiko disk penuh akibat aktivitas deployment


---

# Environment

Server:

VM Backend

Application:

Quilvian V2 Developer Container

Container:

quilviandeveloper-container


---

# 1. Dangling Image Cleanup

## Problem

Setiap deployment Docker dapat menghasilkan image lama tanpa tag:

Example:

<untagged>

Jika tidak dibersihkan, image akan menumpuk dan menggunakan storage Docker.


## Solution

Deployment script:

/opt/QuilvianDeveloper/deploy.sh


ditambahkan:

docker image prune -f


Execution:

Cleanup dijalankan setelah:

1. Container baru berhasil dibuat
2. Health check berhasil


Flow:

Deploy
 |
 v
Start container baru
 |
 v
Health check
 |
 v
Cleanup dangling image


Reason:

Tidak melakukan cleanup sebelum deployment berhasil agar rollback tetap aman.


---

# 2. Docker Container Log Rotation

## Problem

Docker default menggunakan json-file logging tanpa batas ukuran.

Risiko:

/var/lib/docker/containers

dapat membesar dan menyebabkan disk penuh.


## Solution

Docker daemon configuration:

/etc/docker/daemon.json


Configuration:

{
  "log-driver": "json-file",
  "log-opts": {
    "max-size": "50m",
    "max-file": "5"
  }
}


Meaning:

Maximum log:

50MB x 5 file

= ±250MB per container


---

# 3. Docker Compose Logging Policy

Source of truth:

/opt/QuilvianDeveloper/docker-compose.yml


Configuration:

logging:
  driver: json-file
  options:
    max-size: "50m"
    max-file: "5"


Reason:

Docker Compose configuration override daemon default.


---

# 4. Validation

Before Hardening:

Dangling images:

Found multiple untagged images.

Example:

<untagged>
759MB
813MB
813MB
759MB
619MB


Cleanup:

docker image prune -f


Result:

Dangling image:

0


---

# Current Baseline

Docker container logs:

2.5GB

Command:

sudo du -sh /var/lib/docker/containers


Note:

Existing container logs remain until containers are recreated.

Future containers will follow log rotation policy.


---

# Maintenance Recommendation

Weekly:

docker system df


Check:

- Image usage
- Reclaimable storage
- Build cache


Before cleanup:

Investigate active containers.

Avoid:

docker system prune -a


because it may remove rollback images.


---

# Future Improvement

Zero Downtime Deployment:

Current:

docker compose recreate


Future:

Blue-Green Deployment:

- start new container
- health check
- switch traffic
- remove old container
