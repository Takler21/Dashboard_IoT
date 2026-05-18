# Dashboard IoT

Aplicación web para la captación, clasificación automática y explotación
de registros de tráfico IoT

## Acceso online

La aplicación está desplegada y accesible en:
**https://tfg-dashboard-iot.up.railway.app**

Credenciales de prueba:

- Email: administrador@uoc.edu
- Contraseña: admin

## Instalación y ejecución local

> Probado y ejecutado en Windows 11.

> No usar máquinas virtuales de Windows 11, Docker Desktop en Windows virtualiza un entorno Linux. En consecuencia si la máquina no soporta la virtualización anidada no funcionará.

## Requisitos previos

Instalar [Docker Desktop](https://www.docker.com/products/docker-desktop/).

![Opciones de descarga de Docker Desktop según sistema operativo](images/opciones-docker.png)

Se ha usado la versión para Windows AMD64; la versión dependerá del equipo en el que se quiera ejecutar la aplicación.

Dar a _siguiente_ en el instalador hasta finalizar la instalación, y reiniciar el ordenador cuando el instalador lo solicite.

Tras instalar Docker Desktop, abrir PowerShell **como administrador** y ejecutar:

```powershell
wsl --install
```

![Instalación de WSL desde PowerShell](images/wsl.png)

Durante la instalación de WSL se solicitará crear un usuario y contraseña para Ubuntu (Linux). Estas credenciales son independientes de la cuenta de Windows; cualquier nombre y contraseña son válidos para los propósitos del proyecto.

## Descarga y configuración del proyecto

Descargar el archivo ZIP del repositorio: [https://github.com/Takler21/Dashboard_IoT](https://github.com/Takler21/Dashboard_IoT).

![Descarga del repositorio desde GitHub como ZIP](images/repositorio.png)

Extraer el contenido de la carpeta y acceder al directorio raíz del proyecto.

Renombrar el archivo `.env.example` como `.env`.

### Variables de entorno

Obtener las claves para `GEMINI_API_KEY` y `JWT_SECRET_KEY`:

**JWT_SECRET_KEY** — ejecutar el siguiente comando en PowerShell y copiar la clave generada:

```powershell
$bytes = New-Object byte[] 48; (New-Object System.Security.Cryptography.RNGCryptoServiceProvider).GetBytes($bytes); [Convert]::ToBase64String($bytes)
```

**GEMINI_API_KEY** — acceder a [Google AI Studio](https://aistudio.google.com/apikey) con una cuenta de Google y seleccionar la opción para crear una clave API.

![Pantalla de creación de clave API en Google AI Studio](images/clave-gemini.png)

Copiar la clave y pegarla en el archivo `.env`.

### Datos iniciales (opcional)

Previo a la ejecución de los contenedores, para realizar una carga inicial de la base de datos con registros y clasificaciones de ejemplo, eliminar el archivo `init.sql` y renombrar `init2.sql` como `init.sql`. Este paso es opcional; en caso de no realizarlo, la base de datos se iniciará vacía y los registros se podrán cargar directamente desde la aplicación.

## Ejecución

Con el entorno y las variables configuradas, asegurarse de que Docker Desktop esté en ejecución (aparece en la bandeja de tareas de Windows).

Ejecutar en el directorio del proyecto:

- `Instalar.bat` — construye los contenedores por primera vez, o los borra y reconstruye desde cero.
- `Arrancar.bat` — levanta los contenedores y abre el navegador en la aplicación.

La aplicación queda accesible en:

- Frontend: [http://localhost:8080](http://localhost:8080)
- API: [http://localhost:5000](http://localhost:5000)

---

## Ejecución en máquina virtual Ubuntu

> Probado y ejecutado en Ubuntu 24.04 sobre VirtualBox.

### Software necesario

- **VirtualBox:** [https://www.virtualbox.org/wiki/Downloads](https://www.virtualbox.org/wiki/Downloads)
- **Ubuntu 24.04:** [https://releases.ubuntu.com/24.04/](https://releases.ubuntu.com/24.04/)

### Instalación de Docker

Se siguen las instrucciones oficiales de [Docker Engine en Ubuntu](https://docs.docker.com/engine/install/ubuntu/).

#### Añadir el repositorio APT de Docker

```bash
# Añadir la clave GPG oficial de Docker:
sudo apt update
sudo apt install ca-certificates curl
sudo install -m 0755 -d /etc/apt/keyrings
sudo curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
sudo chmod a+r /etc/apt/keyrings/docker.asc
```

Añadir el repositorio a las fuentes de Apt:

```bash
sudo tee /etc/apt/sources.list.d/docker.sources <<'EOF'
Types: deb
URIs: https://download.docker.com/linux/ubuntu
Suites: $(. /etc/os-release && echo "${UBUNTU_CODENAME:-$VERSION_CODENAME}")
Components: stable
Architectures: $(dpkg --print-architecture)
Signed-By: /etc/apt/keyrings/docker.asc
EOF
```

```bash
sudo apt update
```

#### Instalar Docker

```bash
sudo apt install docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
```

#### Verificar que Docker está funcionando

```bash
sudo systemctl status docker
```

Si Docker no está en ejecución, arrancarlo con:

```bash
sudo systemctl start docker
```

#### Añadir el usuario al grupo docker

Para poder ejecutar comandos de Docker sin `sudo`:

```bash
sudo usermod -aG docker $USER
```

Reiniciar el sistema para que el cambio surta efecto.

### Configuración del proyecto

Descomprimir el proyecto. En el directorio raíz, pulsar `Ctrl+H` en el explorador de archivos para mostrar los archivos ocultos.

Editar el archivo `.env.example` con las claves necesarias y renombrarlo a `.env`.

**JWT_SECRET_KEY** — ejecutar el siguiente comando en terminal y copiar la clave generada:

```bash
openssl rand -base64 48
```

**GEMINI_API_KEY** — acceder a [Google AI Studio](https://aistudio.google.com/apikey) con una cuenta de Google y seleccionar la opción para crear una clave API.

![Pantalla de creación de clave API en Google AI Studio](images/clave-gemini.png)

Copiar la clave y pegarla en el archivo `.env`.

### Ejecución de los contenedores

Para limpiar contenedores y volúmenes existentes:

```bash
docker compose down -v
```

Para construir y levantar los contenedores por primera vez (o reconstruir):

```bash
docker compose up -d --build
```

Si los contenedores ya se construyeron anteriormente, para levantarlos:

```bash
docker compose up -d
```

Una vez construidos y levantados los contenedores, acceder desde el navegador a [http://localhost:8080](http://localhost:8080).

---

## Uso

### Acceso

Credenciales de prueba creadas en `init.sql`:

- **Email:** `administrador@uoc.edu`
- **Contraseña:** `admin`

### Carga de registros desde CSV

Si se ha usado `init.sql` por defecto no habrá registros. Pueden añadirse mediante un archivo CSV con un formato compatible, como los que podéis encontrar en [IoT-23 Full Dataset en Kaggle](https://www.kaggle.com/datasets/surajsooraj26/iot-23/data). La descarga desde Kaggle requiere crear una cuenta gratuita.

### Generación de tráfico con el honeypot Cowrie

Conectarse al sensor de Cowrie desplegado mediante:

```bash
ssh root@localhost -p 2222
```

![Conexión SSH al honeypot Cowrie](images/conexion-cowrie.png)

En la primera conexión preguntará si se quiere continuar: indicar `yes`. A continuación, introducir una contraseña genérica como `admin` o `1234` para acceder al sensor.

Una vez dentro, se pueden ejecutar diferentes comandos para generar registros que se enviarán a la aplicación, por ejemplo:

```bash
whoami
uname -a
cat /etc/passwd
```

## Notas

Las estadísticas del dashboard (gráfico y contadores) se actualizan al recargar la vista o al navegar entre pantallas. No se refrescan automáticamente en tiempo real.

### Reinicio del contenedor Cowrie

Si tras haber accedido por primera vez al sensor de Cowrie se reconstruye el contenedor (por ejemplo, al volver a ejecutar `Instalar.bat`), al intentar reconectarse aparecerá un error similar al siguiente:

![Error de host key cambiado al reconectar a Cowrie](images/error-conexion-cowrie.png)

Ejecutar el siguiente comando y volver a conectarse al sensor:

```bash
ssh-keygen -R [localhost]:2222
```
