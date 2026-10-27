#!/usr/bin/env python3
"""Run the BBMB Budget dummy seed (batch DUMMY-SEED:20261022-01) through psql.

Usage (from this directory):
  python3 run_seed.py dry-run            # whole seed in ONE transaction, ends in ROLLBACK
  python3 run_seed.py real               # one transaction per phase, each COMMITs
  python3 run_seed.py verify             # read-only counts (verify.sql)
  python3 run_seed.py file <path.sql>    # run any other SQL file (read-only use only)
  python3 run_seed.py personas-dry-run   # batch DUMMY-SEED:20261022-02 (personas.sql), ROLLBACK
  python3 run_seed.py personas-real      # batch -02, COMMIT
  python3 run_seed.py personas-verify    # read-only (verify_personas.sql)

The connection is read from ConnectionStrings:DefaultConnection in
../../BBMB.BudgetService/appsettings.json. The password is passed to psql via
the PGPASSWORD environment variable only. It is never printed, logged or
written to a file. cleanup.sql is deliberately NOT runnable from here.
"""
import os
import re
import subprocess
import sys
import tempfile

HERE = os.path.dirname(os.path.abspath(__file__))
APPSETTINGS = os.path.normpath(
    os.path.join(HERE, "..", "..", "BBMB.BudgetService", "appsettings.json"))
PSQL = "/opt/homebrew/bin/psql"


def read_connection():
    """Return a dict of connection parts from appsettings.json (comments stripped)."""
    with open(APPSETTINGS, encoding="utf-8") as fh:
        lines = fh.read().splitlines()
    for line in lines:
        if line.strip().startswith("//"):
            continue  # commented-out alternative connection strings
        m = re.search(r'"DefaultConnection"\s*:\s*"([^"]*)"', line)
        if m:
            parts = {}
            for item in m.group(1).split(";"):
                if "=" in item:
                    k, v = item.split("=", 1)
                    parts[k.strip().lower()] = v.strip()
            return parts
    sys.exit("DefaultConnection not found in appsettings.json")


def psql_env(conn):
    env = dict(os.environ)
    env.update({
        "PGHOST": conn.get("host", ""),
        "PGPORT": conn.get("port", "5432"),
        "PGDATABASE": conn.get("database", ""),
        "PGUSER": conn.get("username", conn.get("user id", "")),
        "PGPASSWORD": conn.get("password", ""),
        "PGAPPNAME": "bbmb-dummy-seed-20261022-01",
        "PGCONNECT_TIMEOUT": "15",
    })
    ssl = conn.get("ssl mode", "").lower()
    env["PGSSLMODE"] = {"disable": "disable", "require": "require"}.get(ssl, "prefer")
    return env


def run_psql(sql_file, env, extra_vars=None):
    cmd = [PSQL, "-X", "-q", "-v", "ON_ERROR_STOP=1", "-P", "pager=off"]
    for k, v in (extra_vars or {}).items():
        cmd += ["-v", f"{k}={v}"]
    cmd += ["-f", sql_file]
    return subprocess.run(cmd, env=env, cwd=HERE).returncode


def main():
    if len(sys.argv) < 2 or sys.argv[1] not in ("dry-run", "real", "verify", "file", "personas-dry-run",
                                                 "personas-real", "personas-verify"):
        sys.exit(__doc__)
    mode = sys.argv[1]
    env = psql_env(read_connection())
    # Batch DUMMY-SEED:20261022-02 (dry-run personas, see README "Batch -02").
    if mode.startswith("personas-"):
        env["PGAPPNAME"] = "bbmb-dummy-seed-20261022-02"
        if mode == "personas-verify":
            sys.exit(run_psql(os.path.join(HERE, "verify_personas.sql"), env))
        sys.exit(run_psql(os.path.join(HERE, "personas.sql"), env,
                          {"dry_run": "1" if mode == "personas-dry-run" else "0"}))
    if mode == "verify":
        sys.exit(run_psql(os.path.join(HERE, "verify.sql"), env))
    if mode == "file":
        if len(sys.argv) < 3:
            sys.exit("file mode needs a path")
        if os.path.basename(sys.argv[2]) == "cleanup.sql":
            sys.exit("cleanup.sql is documentation only and must not be executed")
        sys.exit(run_psql(sys.argv[2], env))
    # dry-run / real: seed.sql reads :dry_run to choose ROLLBACK vs per-phase COMMIT
    sys.exit(run_psql(os.path.join(HERE, "seed.sql"), env,
                      {"dry_run": "1" if mode == "dry-run" else "0"}))


if __name__ == "__main__":
    main()
