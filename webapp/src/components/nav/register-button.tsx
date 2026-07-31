import { Button } from "@heroui/button";

/**
 * Sends the user straight to the Keycloak registration form. There is no Auth.js helper for
 * this, so the authorization URL is built by hand against the /registrations endpoint - it takes
 * the same query string as a login. Stays a server component: the target is external.
 */
export default function RegisterButton() {
  const clientId = process.env.AUTH_KEYCLOAK_ID!;
  const issuer = process.env.AUTH_KEYCLOAK_ISSUER!;
  const redirectUrl = process.env.AUTH_URL!;

  const registerUrl =
    `${issuer}/protocol/openid-connect/registrations` +
    `?client_id=${clientId}&redirect_uri=${encodeURIComponent(redirectUrl)}` +
    `&response_type=code&scope=openid`;

  return (
    <Button as="a" href={registerUrl} color="secondary">
      Register
    </Button>
  );
}
