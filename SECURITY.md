# Security Policy

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 0.1.x   | :white_check_mark: |

---

## Scope

UO-MapViewer is a **read-only** tool. It reads Ultima Online client files from disk and renders or exports them. It does not:

- Connect to any network or server
- Write to UO client files
- Execute any game client code
- Collect or transmit user data

As a result, the primary security surface is local file handling and export output.

---

## Reporting a Vulnerability

If you discover a security vulnerability in UO-MapViewer, please **do not open a public GitHub issue**.

Instead, report it privately via one of these methods:

- **GitHub Security Advisories**: Use the [Report a vulnerability](https://github.com/NerdyGamers/UO-MapViewer/security/advisories/new) link in the Security tab of this repo.
- **Email**: Contact the maintainer directly at the email address listed on the GitHub profile.

Please include:
- A description of the vulnerability and its potential impact
- Steps to reproduce or a proof-of-concept
- The version of UO-MapViewer affected
- Any relevant environment details (.NET version, OS, client file format)

---

## Response Timeline

| Step                        | Target Time   |
| --------------------------- | ------------- |
| Acknowledgement of report   | Within 72 hours |
| Initial assessment          | Within 7 days |
| Fix or mitigation published | Within 30 days (where applicable) |

---

## Responsible Disclosure

We ask that you:
- Give us reasonable time to address the issue before public disclosure
- Avoid accessing or modifying data that does not belong to you during research
- Act in good faith

We commit to:
- Acknowledging your report promptly
- Crediting reporters in release notes (unless you prefer to remain anonymous)
- Not pursuing legal action for good-faith security research

---

## Out of Scope

- Vulnerabilities in the Ultima Online client itself
- Issues requiring physical access to the user's machine
- Social engineering attacks
