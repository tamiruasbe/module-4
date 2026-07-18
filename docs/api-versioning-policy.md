# API Versioning Policy

## Purpose

The purpose of this policy is to ensure that the TMS API can evolve without breaking existing client applications. New versions will be introduced whenever breaking changes are required, while non-breaking improvements will be added to the current version whenever possible.

---

## 1. Breaking Changes

A breaking change is any modification that requires existing clients to change their code before they can continue using the API.

Examples of breaking changes include:

- Removing an existing field from a response.
- Renaming an existing field.
- Changing the data type of a field.
- Changing the HTTP status code returned by an endpoint.
- Making validation rules stricter, such as changing an optional field into a required field.
- Changing the default sorting or paging behavior.
- Removing or renaming an existing endpoint.

Whenever a breaking change is introduced, a new API version must be created.

---

## 2. Non-Breaking (Additive) Changes

A non-breaking change allows existing clients to continue working without modification.

Examples include:

- Adding a new optional field to a response.
- Adding a new endpoint.
- Adding a new optional query parameter.
- Improving performance without changing the API contract.
- Adding additional documentation.

These changes do not require a new API version.

---

## 3. Sunset Policy

When a new major API version is released, the previous version will continue to be supported for at least **6 months**.

This gives all clients, including organizations with scheduled maintenance periods, enough time to migrate safely to the new version.

After the sunset period ends, the older version may be removed from service.

---

## 4. Communication Strategy

When an API version is deprecated, clients will be notified using several methods:

- The **Deprecation** HTTP response header.
- The **Sunset** HTTP response header showing the retirement date.
- The **Link** HTTP response header pointing to the replacement version.
- An entry in the project CHANGELOG.
- Email notifications sent to all registered API consumers.
- Calendar reminders before the shutdown date.

These communication methods ensure that clients receive sufficient notice before support ends.

---

## 5. Version Upgrade Policy

Clients are allowed to upgrade directly from one supported version to any newer supported version.

For example:

- Version 1 → Version 2
- Version 1 → Version 3

Clients are **not required** to upgrade through every intermediate version.

---

## Summary

The TMS API follows a versioning strategy that protects existing clients while allowing the API to evolve. Breaking changes always require a new version, while non-breaking improvements can be added safely. A clear deprecation policy and communication plan help clients migrate successfully before older versions are retired.
