# Migrating from Stormpath to Okta

This document is intended to give you a high-level understanding of the steps required to migrate an application from Stormpath to Okta. For more information, see the [Stormpath-Okta Customer FAQ](https://stormpath.com/oktaplusstormpath).

The Stormpath API will sunset on **2017-08-17 19:00 UTC** (August 17, 2017 at noon PDT). At the very least, make sure you export your data before then!

## Before you begin

* Read the [changelog](changelog.md) for information on breaking changes.

* The data import tool (step 4) is currently being polished and will be released soon. Until then, you can create some test data in your Okta organization using the [Test Data](https://github.com/stormpath/express-stormpath/blob/4.0.0/util/okta-test-data.js) script from the express-stormpath project:

```
git clone https://github.com/stormpath/express-stormpath.git --branch 4.0.0
cd express-stormpath
npm install
node util/okta-test-data.js --apiToken=TOKEN --org=https://dev-YOUR_ORG.oktapreview.com/
```

## Migration steps

1. Create a test user in your existing system with a known password.
1. [Export](https://stormpath.com/export) your Stormpath data. (link TODO)
1. Sign up for a [new developer Okta organization](https://www.okta.com/developer/signup/stormpath/), even if you already have an Okta organization. Signing up through this link will provision an Okta organization with features most similar to Stormpath.
1. Import your Stormpath data into Okta. (coming soon)
1. Upgrade the version of `Stormpath.AspNet`, `Stormpath.AspNetCore`, or `Stormpath.Owin` in your project to 4.0.0 or greater. (You may need to check the "Show prerelease" box in order to see the RC releases.)
1. Remove any references to `Stormpath.SDK` or the `IClient` interface. The Stormpath .NET SDK has been deprecated. Any code that was using the SDK will need to be refactored to use the [Okta .NET SDK](https://github.com/okta/oktasdk-csharp) or [Okta REST API](http://developer.okta.com/docs/api/getting_started/api_test_client.html). If you need help, let us know at support@stormpath.com.
1. Update your application configuration:

	* You must provide these new configuration properties:
		* `org` (your Okta org URL, like `https://dev-123456.oktapreview.com`),
		* `apiToken` (an Okta [API token](http://developer.okta.com/docs/api/getting_started/getting_a_token.html)),
		* `application.id` (the Okta application ID, which can be found in the URL of the Admin UI when editing the Application: `/admin/app/oidc_client/instance/<appid>`)

	* If you were using `STORMPATH_*` environment variables to set any configuration properties, you'll need to update them to `OKTA_*`.

	* Most of the remaining configuration can be left untouched. See the [configuration breaking changes](todo) (TODO).
	
	* A minimal example configuration looks like:
	
	```csharp
	Configuration = new StormpathConfiguration
	{
	    Org = "https://dev-12345.oktapreview.com/",
	    ApiToken = "my-api-token",
	    Application = new OktaApplicationConfiguration()
	    {
		Id = "abcdef-123"
	    }
	}
	```

1. **If** you used the Password Reset workflow in Stormpath, update the Okta Password Reset email template. You can copy the current template from the Stormpath Admin Console, and paste it into the Okta template found at Settings > Email & SMS > Forgot Password.  You'll want to use the ``${recoveryToken}`` variable to create a link that points the user to the change password endpoint on your application, for example: ``http://localhost:3000/change?sptoken=${recoveryToken}``. If the validator complains about `${resetPasswordLink}` being missing, place it in an HTML comment: `<!-- ${resetPasswordLink} -->`

1. **If** you used the Email Verification workflow in Stormpath, you'll need to use the `SendEmailVerificationHandler` in this library, plus an email service like SendGrid, to send your own verification email.

1. Run your application with your configuration pointed to the new Okta organization that contains your imported data, and try logging in with the test user you created in step 1.

If you run into problems, please let us know at support@stormpath.com. We'll be continually updating this library (and document) as needed to help make the migration process as smooth as possible.