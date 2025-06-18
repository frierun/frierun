# Installing Cloudflare tunnels

## Create API token

Go to the [Cloudflare dashboard](https://dash.cloudflare.com/profile/api-tokens)

<details>
<summary>Click on "Create Token"</summary>

![API Token screenshot](Cloudflare1.png)
</details>

<details>
<summary>Click on "Create Custom Token"</summary>

![Create Custom Token screenshot](Cloudflare2.png)
</details>

Type a name for the token and select the following permissions for the token:
- Account: Cloudflare Tunnel: Edit
- Account: DNS Views: Read
- Zone: DNS: Edit
 
<details>
<summary>As shown on the screenshot</summary>

![Token select screenshot](Cloudflare3.png)
</details>

You can leave other fields as default, or set restrictions to what zones and accounts the token can access.

Press "Continue to summary" and then "Create Token". 

<details>
<summary>You will be shown the token, copy it and save it somewhere safe. You will not be able to see it again.</summary>

![Token created screenshot](Cloudflare4.png)

</details>

## Register or move domain

You can either register a new domain with Cloudflare or move an existing domain to Cloudflare.
This domain will be used to access your server.

Open the [Cloudflare dashboard](https://dash.cloudflare.com/?to=/:account/home) and follow the instructions to add a new site or move an existing one.

## Install cloudflare-tunnel application

In the Frierun web interface select `cloudflare-tunnel` from the list of provider packages and press `Install`.

Enter the API token you created earlier.

![Install cloudflare-tunnel screenshot](Cloudflare5.png)

## Use it

You can now expose other applications to the internet using Cloudflare tunnels. When installing other application, select `Cloudflare` as Http endpoint.

![Install application with CloudflareHandler screenshot](Cloudflare6.png)