# Persistence fixtures

`echo-v1.json` is an actual checksummed save produced by commit
`4d8a69d7f74b62ba10a8ebe8930fc4c7215606ae` during its isolated validation.
It contains synthetic test-profile UUIDs, level 3 / 7 XP, completed Q1, an
unresolved quest and 40% HP. Do not regenerate it with the current serializer:
its purpose is to prove compatibility with the previously pushed v1 format.

`SaveMigrationValidation` adds opaque section fixtures in a temporary directory
and tests failure/recovery without altering this source file.
