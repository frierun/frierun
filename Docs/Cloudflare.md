# Installing Cloudflare tunnels

## Create API token

Go to the [Cloudflare dashboard](https://dash.cloudflare.com/profile/api-tokens)

<div style="background-color: #f0f0f0; padding: 10px; border-radius: 8px; display: inline-block;">

![API Token screenshot](Cloudflare1.png)

</div>

Select "Create Token" and then "Create Custom Token". 

<div style="background-color: #f0f0f0; padding: 10px; border-radius: 8px; display: inline-block;">

![Create Custom Token screenshot](Cloudflare2.png)

</div>

Type a name for the token and select the following permissions for the token:
- Account: Cloudflare Tunnel: Edit
- Account: DNS Views: Read
- Zone: DNS: Edit

<div style="background-color: #f0f0f0; padding: 10px; border-radius: 8px; display: inline-block;">
 
![Token select screenshot](Cloudflare3.png)

</div>

You can leave other fields as default, or set restrictions to what zones and accounts the token can access.

Press "Continue to summary" and then "Create Token". 
You will be shown the token, copy it and save it somewhere safe. You will not be able to see it again.

<div style="background-color: #f0f0f0; padding: 10px; border-radius: 8px; display: inline-block;">

![Token created screenshot](Cloudflare4.png)

</div>

## Register or move domain

You can either register a new domain with Cloudflare or move an existing domain to Cloudflare.
This domain will be used to access your server.

Open the [Cloudflare dashboard](https://dash.cloudflare.com/?to=/:account/home) and follow the instructions to add a new site or move an existing one.

## Install cloudflare-tunnel application

In the Frierun web interface select `cloudflare-tunnel` from the list of provider packages and press `Install`.

Enter the API token you created earlier.

<div style="background-color: #f0f0f0; padding: 10px; border-radius: 8px; display: inline-block;">

![Install cloudflare-tunnel screenshot](Cloudflare5.png)

</div>

## Use it

You can now expose other applications to the internet using Cloudflare tunnels. When installing other application, select `Cloudflare` as Http endpoint.

<div style="background-color: #f0f0f0; padding: 10px; border-radius: 8px; display: inline-block;">

![Install application with CloudflareHandler screenshot](Cloudflare6.png)

</div>