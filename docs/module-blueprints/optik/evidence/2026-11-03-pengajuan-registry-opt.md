# Pengajuan Baris Registry — Optik (`Opt`)

| Field | Nilai |
| --- | --- |
| Evidence ID | `OPT-EVD-001` |
| Task roadmap | `BE-OPT-000` (`roadmap/backend-roadmap.md`) |
| Status | **Disetujui dan tercatat** 2026-11-03 — Muhammad Hamzah (pemilik registry), disampaikan Yoga Aji Pratama |
| Pengaju | Yoga Aji Pratama, Product/Domain Owner Optik (`OPT-DEC-002`) |
| Dasar | Blueprint `OPT-BP-001` r1 `approved` 2026-11-03; keputusan `OPT-DEC-036`; `FR-OPT-001`; QBE-MOD-002, QBE-MOD-003, QBE-NAM-004 |

## Langkah prosedur (`MODULE_OWNERSHIP_PREFIX_REGISTRY.md` — *Prosedur pendaftaran modul/prefix baru*)

| Langkah | Hasil |
| --- | --- |
| 1. Penempatan | `Areas/HealthServices/OpticalManagement/`; configuration di `Repositories/Configurations/HealthServices/OpticalManagement/` (`02-backend-architecture.md` bagian 5) |
| 2. Cek registry | Belum ada pemilik untuk layanan Optik (`OPT-CAP-09`) |
| 3. Prefix | `Opt` = *Optical* — tiga huruf, PascalCase, belum dipakai baris lain. Diverifikasi 2026-11-03 (SHA `2a5094ca`): tidak ada class, tabel, maupun migration bernama `Opt*` di `Areas/`, `Models/`, `Repositories/`, `Migrations/` |
| 4. Baris yang diajukan | `HealthServices` / `OpticalManagement / Optical` / `BUSINESS DOMAIN / MODULE` / `Opt` / `ACTIVE` |
| 5. Persetujuan | ✅ Muhammad Hamzah, 2026-11-03; baris ditulis ke registry pada tanggal yang sama |
| 6. Model pertama | `BE-OPT-002`, sesudah langkah 5 |

**Lifecycle langsung `ACTIVE`** mengikuti preseden `Num`, `Hmd`, `Mcu`, `Mdf`, dan `Nvw`:
`Resolve-RegistryOwnership` hanya menerima token lifecycle `active` dan `legacy`.

## Baris tabel yang diusulkan

```markdown
| HealthServices | OpticalManagement / Optical | BUSINESS DOMAIN / MODULE | Opt | ACTIVE |
```

Tabel kepanjangan prefix:

```markdown
| Opt | Optical |
```

## Usulan catatan perubahan lifecycle

```markdown
| 2026-11-03 | OpticalManagement / Optical / `Opt` | Baris baru — pendaftaran Area `HealthServices`, pemilik `OpticalManagement / Optical`, prefix `Opt` = *Optical*, lifecycle langsung `ACTIVE` | <Penyetuju>, <peran>. Persetujuan disampaikan Yoga Aji Pratama selaku Product/Domain Owner Optik (`OPT-DEC-002`) pada sesi <tanggal>. Dasar: blueprint `OPT-BP-001` (`docs/module-blueprints/optik/`), keputusan `OPT-DEC-036`, task `BE-OPT-000`. Lifecycle langsung `ACTIVE` mengikuti preseden `Num`, `Hmd`, `Mcu`. Mencabut penghalang QBE-MOD-002/QBE-MOD-003 atas entity operasional `Opt*` pertama di `Areas/HealthServices/OpticalManagement/`. Entity master milik modul ini (`OptItemProfile`, `OptSetting`) memakai `Opt`, bukan `Mst`, karena hanya bermakna di unit Optik (pola `Hmd`, `Mcu`). Wewenangnya penamaan dan kepemilikan saja; tidak memberi wewenang migration, eksekusi database, maupun deployment. Salinan canonical suite skill perlu diselaraskan pemegang registry. |
```

## Catatan untuk penyetuju

| Butir | Isi |
| --- | --- |
| Jebakan sub-folder `MasterData` | Optik **tidak** memakai sub-folder `MasterData` di dalam `OpticalManagement`, sehingga tidak berbenturan dengan alias baris `Mst` (pola masalah `Mdf`, catatan 2026-10-26) |
| Siapa yang menyetujui | Langkah 5 prosedur meminta persetujuan pemilik modul, dan ada preseden baris yang ditulis pemilik modul sendiri (`Inp`, `Bbk`, `Acc`, `Emg`, `Nvw`). Untuk `Pat`/Membership dan `Mcu`, Yoga Aji Pratama menyampaikannya ke pemilik registry Muhammad Hamzah. Pilihan jalurnya keputusan pengaju |
| Kontrol negatif | Sesudah baris tercatat, checker QBE dijalankan dengan model kontrol `Zzz*` di folder tak terdaftar yang **harus** terkena QBE-MOD-002 |

## Bukti checker (2026-11-03)

| Run | Berkas | QBE-MOD-002 | Catatan |
| --- | --- | --- | --- |
| 1 dan 2 (pemanggilan terpisah) | `Areas/HealthServices/OpticalManagement/Models/OptRegistryProbe.cs` (`[Table]`, `IdentityModel`) | Tidak terkena | QBE-CFG-001 wajar (probe tanpa configuration) |
| 1 dan 2 | `Areas/HealthServices/ZzzProbeManagement/Models/ZzzRegistryProbe.cs` (kontrol negatif) | **Terkena** | Membuktikan checker menilai QBE-MOD-002 |

Kedua probe dihapus sesudahnya; `git status --short Areas/` bersih.
