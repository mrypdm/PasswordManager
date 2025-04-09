HOST="$1"
IP="$2"

openssl req \
    -x509 \
    -newkey rsa:4096 \
    -sha256 \
    -days 365 \
    -nodes \
    -keyout key.key \
    -out cert.crt \
    -subj "/CN=${HOST}" \
    -extensions v3_ca \
    -extensions v3_req \
    -config <( \
      echo '[req]'; \
      echo 'default_bits= 4096'; \
      echo 'distinguished_name=req'; \
      echo 'x509_extension = v3_ca'; \
      echo 'req_extensions = v3_req'; \
      echo '[v3_req]'; \
      echo 'basicConstraints = CA:FALSE'; \
      echo 'keyUsage = nonRepudiation, digitalSignature, keyEncipherment'; \
      echo 'subjectAltName = @alt_names'; \
      echo '[ alt_names ]'; \
      echo "DNS.1 = ${HOST}"; \
      echo "IP.1 = ${IP}"; \
      echo '[ v3_ca ]'; \
      echo 'subjectKeyIdentifier=hash'; \
      echo 'authorityKeyIdentifier=keyid:always,issuer'; \
      echo 'basicConstraints = critical, CA:TRUE, pathlen:0'; \
      echo 'keyUsage = critical, cRLSign, keyCertSign'; \
      echo 'extendedKeyUsage = serverAuth, clientAuth')

openssl x509 -noout -text -in cert.crt
