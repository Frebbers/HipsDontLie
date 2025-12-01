# Project HDL
This repository is a fork of the Project GameTogether backend using a Blazor based front end. https://github.com/frebbers/gametogether-backend

# How to deploy this project

There are two ways of deploying HipsDontLie. Using Github Actions (recommended) and manually, using a .env file. Regardless of which one you choose, you must set up your nginx siteconf manually, as the swag container will only handle SSL certificates manually.

# Prerequisites
* 64 bit Linux system with SSH public key authorization enabled
* Docker
* non-root docker user with ssh privileges and an ssh public key
* UFW (optional but recommended)

# Method 1: Automatic deployment with GitHub Actions
If you want to customize your HipsDontLie instance with, for instance, your own branding, it is recommended to fork this repository and add your environment variables to GitHub actions to deploy.

## Step 1: Set up GitHub Actions variables
* DOCKERHUB_USERNAME
* HDL_AZURE
* EMAIL_VERIFICATION_URL
* HDL_DOMAIN
* HOST_DOMAIN
* MONGO_DATABASE_NAME
* MONGO_DATABASE_USERNAME
* MYSQL_DATABASE_NAME
* MYSQL_DATABASE_USER_NAME
* SMTP_PORT
* SMTP_SENDER_EMAIL
* HDL_AZURE
* SMTP_SERVER

## Step 2: Set up GitHub Actions secrets

* DOCKERHUB_TOKEN
* GOOGLE_CLIENT_ID
* GOOGLE_CLIENT_SECRET
* JWT_SECRET
* MONGO_DATABASE_PASSWORD
* MONGO_ROOT_PASSWORD
* MONGO_ROOT_USERNAME
* MYSQL_DATABASE_PASSWORD
* MYSQL_ROOT_PASSWORD
* SMTP_SENDER_PASSWORD
* SSH_KEY_PASS_PHRASE
* SSH_PORT
* SSH_PRIVATE_KEY
* SSH_USER

## Step 3: Trigger deployment

Make any change to any of the directories beginning with HipsDontLie to trigger deployment. Watch your runner automatically deploy the project! 

# Method 2: Manual Deployment with Docker Compose

If you don't want to fork the Github Repository and just run it as-is, follow these steps.

## Step 1: Clone the project
```git clone https://github.com/Frebbers/HipsDontLie```

## Step 2: Create your env file
```nano .env```

## Step 3: Populate your env file
Use this template to populate your .env file
```
# JWT settings
JWT_SECRET=your_jwt_secret_here
HOST_DOMAIN=your_host_domain_here
CERT_NOTIFICATION_EMAIL=your_cert_notification_email_here

# Email settings
EMAIL_VERIFICATION_URL=your_email_verification_url_here
SMTP_SERVER=your_smtp_server_here
SMTP_PORT=your_smtp_port_here
SENDER_EMAIL=your_smtp_sender_email_here
SENDER_PASSWORD=your_smtp_password_here

# OAuth settings
GOOGLE_CLIENT_ID=your_google_client_id_here
GOOGLE_SECRET=your_google_client_secret_here

# MySQL database settings
MYSQL_DATABASE_NAME=your_mysql_database_name_here
DB_NAME=your_mysql_database_name_here
DB_USER=your_mysql_user_here
DB_PASSWORD=your_mysql_password_here
DB_ROOT_PASSWORD=your_mysql_root_password_here

# MySQL connection string for the DB container
# Format: mysql://user:password@host:port/database
DB_CONNECTION_STRING=mysql://DB_USER:DB_PASSWORD@db:3306/DB_NAME


# MongoDB settings
MONGO_INITDB_ROOT_USERNAME=your_mongo_root_username_here
MONGO_INITDB_ROOT_PASSWORD=your_mongo_root_password_here
MONGO_INITDB_DATABASE=your_mongo_database_name_here
MONGO_DATABASE_USERNAME=your_mongo_database_user_here
MONGO_DATABASE_PASSWORD=your_mongo_database_password_here

# MongoDB connection string for the DB container
# Format: mongodb://user:password@host:port/database?authSource=admin
MongoChat__ConnectionString=mongodb://MONGO_DATABASE_USERNAME:MONGO_DATABASE_PASSWORD@mongodb:27017/MONGO_INITDB_DATABASE?authSource=admin
```
## Step 5: Start HDL
```docker compose -f compose.prod.yaml up -d```
You can now access HDL at whatever port you configured in your siteconf file.

