# Human Resource — Existing Capability Map (penunjuk)

**Berkas ini bukan capability map. Berkas ini penunjuk.**

Capability map modul HR yang berlaku ada di
[`01-existing-capability-map.md`](./01-existing-capability-map.md), revision `1.1`, status
`source-audited`.

---

## Kenapa ada dua nama

Ada dua kontrak yang sama-sama mengikat dan memberi nomor berbeda untuk dokumen yang sama:

| Sumber aturan | Nama yang diminta |
| --- | --- |
| `docs/module-blueprints/_template/` | `02-existing-capability-map.md` |
| `blueprint-output-contract.md` milik `design-business-module` | `01-existing-capability-map.md` |

Modul yang sudah lebih dulu jalan — `rawat-inap` dan `billing-kasir` — memakai
`01-existing-capability-map.md`. Modul HR mengikuti keduanya: nama dari template tetap
dimaterialisasi supaya struktur folder tidak timpang, tetapi isinya hanya menunjuk, tidak
menyalin.

## Aturan yang berlaku

1. **Satu sumber kebenaran.** [`01-existing-capability-map.md`](./01-existing-capability-map.md)
   adalah satu-satunya capability map modul HR.
2. **Jangan menyalin isi ke sini.** Dua berkas berisi peta kemampuan yang sama akan berbeda
   isinya cepat atau lambat, dan pembaca berikutnya tidak punya cara menentukan mana yang benar.
3. **Jangan memperbarui berkas ini** kecuali nama berkas canonical-nya berubah.

## Ringkasan sekilas

Untuk pembaca yang hanya butuh gambaran, ini isi ringkas capability map yang sebenarnya:

| Ukuran | Nilai |
| --- | --- |
| Controller HR backend | 150 |
| Endpoint HR backend | 1.343 |
| Endpoint yang dipanggil frontend | 81 |
| Endpoint operasional tanpa pemakai | ± 577 |
| Model HR | 337 |
| Model di enam domain tanpa controller | 68 |
| Benar-benar belum punya API | 67 |
| Test yang menyentuh HR | 0 |

Angka-angka itu beserta bukti barisnya ada di dokumen canonical. Jangan mengutip tabel ini
sebagai bukti; kutip dokumen aslinya.
