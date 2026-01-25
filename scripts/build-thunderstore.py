#!/usr/bin/env python3
import json
import re
from pathlib import Path


def read_version(plugin_path: Path) -> str:
    content = plugin_path.read_text(encoding="utf-8")
    match = re.search(
        r'\[BepInPlugin\(".*?",\s*".*?",\s*"([^"]+)"\)\]',
        content,
    )
    if not match:
        raise RuntimeError("Could not find version in EasySpawnerPlugin.cs")
    return match.group(1)


def update_manifest(manifest_path: Path, version: str) -> None:
    data = json.loads(manifest_path.read_text(encoding="utf-8"))
    data["version_number"] = version
    manifest_path.write_text(
        json.dumps(data, indent=4) + "\n",
        encoding="utf-8",
    )


def update_ts_readme(readme_path: Path, ts_readme_path: Path) -> None:
    ts_readme_path.write_text(
        readme_path.read_text(encoding="utf-8"),
        encoding="utf-8",
    )


def main() -> None:
    repo_root = Path(__file__).resolve().parents[1]
    plugin_path = repo_root / "EasySpawner" / "EasySpawnerPlugin.cs"
    manifest_path = repo_root / "Thunderstore" / "manifest.json"
    readme_path = repo_root / "README.md"
    ts_readme_path = repo_root / "Thunderstore" / "TS-README.md"

    version = read_version(plugin_path)
    update_manifest(manifest_path, version)
    update_ts_readme(readme_path, ts_readme_path)

    print(f"Updated Thunderstore manifest + TS-README for {version}")


if __name__ == "__main__":
    main()
