# Changelog

Migration information TODO.

Please read the following breaking changes carefully. Some applications will work as-is, and some will require refactoring. If you have questions or need help, please reach out to us at support@stormpath.com!

## Breaking changes

* The Stormpath SDK has been removed. If you weren't accessing the SDK directly, this shouldn't impact you. If you were, you will need to refactor the relevant code to use the Okta .NET SDK or REST API.
* Because of this, the SDK `IAccount` interface is no longer used to represent a Stormpath account profile. To help cope with this change,
	* The user profile will be represented by `dynamic` instead of `IAccount`
	* The `dynamic` account will contain the new Okta profile fields under `account.profile`, and **also**
	* The old Stormpath top-level profile fields will be kept wherever possible
	* CustomData _todo_
* The `/forgot` and `/change` routes are now disabled by default. Previously, they would follow the Stormpath Directory configuration by default. The routes can be enabled or disabled by changing the `okta.web.forgotPassword.enabled` or `okta.web.changePassword.enabled` settings.
* The shape of the user data passed to the `PreChangePasswordHandler` and `PostChangePasswordHandler` has changed. It is now an Okta profile map, as seen in this example: http://developer.okta.com/docs/api/resources/authn.html#verify-recovery-token
* Okta handles custom profile fields differently than Stormpath. Any custom profile field you want to use must be defined in advance in the Universal Directory. Otherwise, you will get API errors when creating a user with a custom profile field.
* The custom profile field `stormpathMigrationRecoveryAnswer` (string) must be defined in your Okta Universal Directory. This package uses it internally for self-service password reset.
* The `StateTokenBuilder` and `StateTokenParser` classes were moved from `Stormpath.Owin.Abstractions` to `Stormpath.Owin.Middleware`.
* The `*FormViewModelBuilder` classes was moved from `Stormpath.Owin.Abstractions` to `Stormpath.Owin.Middleware`.
* Okta uses an API Token to authenticate calls to the Okta API, similar to Stormpath's API Key ID/Secret.  However, unlike Stormpath API credentials, Okta API Tokens will expire in 30 days if they are not used. This means you will get an API error if your application has not been started in 30 days. If this happens, you can generate a new API Token in the Okta Admin dashboard.