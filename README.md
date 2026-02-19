# Chickquita

## Local development setup

After cloning, activate the pre-commit hook (one-time per machine):

```bash
git config core.hooksPath .husky
```

This runs `npm run validate` (ESLint + TypeScript type-check) before every commit, matching what CI checks.

To run the check manually at any time:

```bash
cd frontend && npm run validate
```
