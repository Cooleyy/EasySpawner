"""Release metadata checks; run with python3 -m unittest discover -s scripts -p 'test_*.py'."""
import json
import re
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


class ReleaseMetadataTests(unittest.TestCase):
    def test_versions_agree(self):
        manifest = json.loads((ROOT / 'Thunderstore/manifest.json').read_text())
        version = manifest['version_number']
        plugin = (ROOT / 'EasySpawner/EasySpawnerPlugin.cs').read_text()
        match = re.search(r'\[BepInPlugin\("[^"]+",\s*"[^"]+",\s*"([^"]+)"\)', plugin)
        assert match is not None, 'Missing BepInPlugin version'
        self.assertEqual(match.group(1), version)
        assembly = (ROOT / 'EasySpawner/Properties/AssemblyInfo.cs').read_text()
        for attribute in ('AssemblyVersion', 'AssemblyFileVersion'):
            match = re.search(attribute + r'\("([0-9.]+)"\)', assembly)
            assert match is not None, 'Missing ' + attribute
            self.assertEqual(match.group(1), version + '.0', attribute)
        for file in ('README.md', 'Thunderstore/TS-README.md'):
            self.assertIn('#### ' + version, (ROOT / file).read_text())


if __name__ == '__main__':
    unittest.main()
