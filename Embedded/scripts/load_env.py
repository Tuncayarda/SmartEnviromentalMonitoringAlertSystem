"""
PlatformIO pre-build script: reads .env and injects values as C/C++ preprocessor defines.

For every KEY=VALUE line the script creates a macro named KEY_VAL:
  - Numeric values  → -DKEY_VAL=1883
  - String values   → -DKEY_VAL=\"hello\"

Usage in platformio.ini:
    extra_scripts = pre:scripts/load_env.py
"""

Import("env")  # noqa: F821  (PlatformIO SCons environment)

import os


def _is_numeric(value: str) -> bool:
    """Return True only for plain integers/floats with no leading zeros.

    '001' looks numeric but is a string ID — leading zero means string.
    '1883' or '3.14' are genuine numbers.
    """
    if not value:
        return False
    try:
        int_val = int(value)
        # Leading zeros (e.g. "001") → treat as string to preserve them
        if str(int_val) != value:
            return False
        return True
    except ValueError:
        pass
    try:
        float(value)
        return True
    except ValueError:
        return False


def load_env_file():
    project_dir = env.subst("$PROJECT_DIR")  # noqa: F821
    env_path = os.path.join(project_dir, ".env")

    if not os.path.isfile(env_path):
        print(
            "\033[93mWARNING: .env file not found. "
            "Copy .env.example to .env and fill in your values.\033[0m"
        )
        return

    defines = []
    with open(env_path, encoding="utf-8") as f:
        for raw_line in f:
            line = raw_line.strip()
            if not line or line.startswith("#"):
                continue
            if "=" not in line:
                continue

            key, _, value = line.partition("=")
            key = key.strip()
            value = value.strip()

            macro_name = f"{key}_VAL"
            if _is_numeric(value):
                defines.append((macro_name, value))
            else:
                # Escape the quotes so the C string literal is correct.
                defines.append((macro_name, f'\\"{value}\\"'))

    if defines:
        env.Append(CPPDEFINES=defines)  # noqa: F821
        print(f"load_env.py: loaded {len(defines)} define(s) from .env")


load_env_file()
