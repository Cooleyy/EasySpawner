#!/usr/bin/env python3
import re
import shutil
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


def main() -> None:
    repo_root = Path(__file__).resolve().parents[1]
    plugin_path = repo_root / "EasySpawner" / "EasySpawnerPlugin.cs"
    thunderstore_dir = repo_root / "Thunderstore"
    version = read_version(plugin_path)

    dist_dir = repo_root / "dist"
    package_dir = dist_dir / "thunderstore"
    if package_dir.exists():
        shutil.rmtree(package_dir)
    shutil.copytree(thunderstore_dir, package_dir)
    ts_readme_copy = package_dir / "TS-README.md"
    readme_path = package_dir / "README.md"
    if ts_readme_copy.exists():
        if readme_path.exists():
            readme_path.unlink()
        ts_readme_copy.rename(readme_path)

    dll_candidates = [
        repo_root / "EasySpawner" / "bin" / "Release" / "EasySpawner.dll",
        repo_root / "EasySpawner" / "bin" / "Debug" / "EasySpawner.dll",
    ]
    dll_path = next((path for path in dll_candidates if path.exists()), None)
    if dll_path is None:
        raise RuntimeError("EasySpawner.dll not found. Build the project first.")

    shutil.copy2(dll_path, package_dir / "EasySpawner.dll")

    zip_path = dist_dir / f"EasySpawner_Thunderstore_{version}.zip"
    if zip_path.exists():
        zip_path.unlink()
    shutil.make_archive(str(zip_path.with_suffix("")), "zip", package_dir)

    print(f"Built Thunderstore zip: {zip_path}")


if __name__ == "__main__":
    main()
