## HTTP Auth Key
This key can be of any length

`
openssl genrsa -out http_auth_key.pem 2048
`

Take the base64 string, and make it into 1 line. That string is value to set APIKeyPrivateKey to.

## Presence Preauth Key

This key should be 512 bits, otherwise some games may not work properly, due to exceeding the maximum size for auth tokens. Most games can handle longer, but some such as Battlefield 2142 cannot.

`
openssl genrsa -out preauth_key.pem 512
`

## Getting a key

This next part needs improvement, but basically comment out the Authorize attribute in the APIKeyController, and make a request to GenerateKey to make an admin key. This is for your first key, any key made after will not require this, so its easier to just do it locally.

`
curl -X POST "http://localhost:5000/api/APIKey/GenerateKey" -H  "accept: text/plain" -H  "Content-Type: application/json-patch+json" -d "{  \"name\": \"dev test key\",  \"roles\": [    \"Admin\"  ]}"
`