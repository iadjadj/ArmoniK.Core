terraform {
  required_providers {
    docker = {
      source  = "kreuzwerker/docker"
      version = ">= 3.9.0"
    }
    pkcs12 = {
      source  = "chilicat/pkcs12"
      version = ">= 0.4.0"
    }
  }
}