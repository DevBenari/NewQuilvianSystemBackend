# Laporan Perubahan Frontend — `FE-BKC-FIX-014`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BKC-FIX-014` |
| Judul | Tambahkan No. Kunjungan + tanggal/jam ke label opsi dropdown Pasien/Kunjungan (Tambah Invoice Manual) |
| Sumber | [`PLAN-REPAIR-BKC-003`](../../../docs/plan-repair/plan-repair-003-kunjungan-pasien-tampil-identik.md) `FIX-BKC-003-01`; temuan `ISS-BKC-003-01` |
| Keputusan | K-01 (format label: No. Kunjungan + tanggal+jam) — diputuskan pemilik modul Billing 16-10-2026 |
| Dependency | Tidak ada. Field `EncounterNumber`/`EncounterDate` sudah dikirim penuh oleh backend (`ActiveEncounterOptionResponse`), tidak ada perubahan backend/kontrak API |
| Wewenang UI | `REUSE` murni — tidak ada base component baru atau diubah. Perubahan hanya pada string label yang dikonsumsi `FilterSelect`/`BaseEditorForm` yang sudah ada |
| Target tulis | `QuilvianSystemFrontendDev` |
| Branch | `yasmina` (working tree, belum di-commit) |
| Tanggal | 2026-10-16 |
| Status | ✅ **Source selesai**; lint 0 error; `npm run build` lulus (exit 0). Uji manual live browser `NOT FEASIBLE` — lihat bagian 4 |

## 1. Masalah

Dropdown "Pasien / Kunjungan" pada layar testing **Tambah Invoice Manual** menampilkan label opsi
yang hanya terdiri dari nama pasien, No. RM, jenis layanan, dan cara bayar. Field `EncounterNumber`
dan `EncounterDate` sudah dikirim penuh oleh backend tapi tidak pernah dibaca di titik pembentukan
label. Akibatnya, saat pasien yang sama punya lebih dari satu kunjungan aktif dengan kombinasi
nama+No.RM+jenis layanan+cara bayar yang sama (skenario nyata, dikonfirmasi dengan data produksi
pada `issue-003` — tiga `RegPatientEncounter` berbeda untuk satu pasien), ketiganya tampil sebagai
baris yang terlihat identik di dropdown, sehingga kasir tidak bisa memastikan kunjungan mana yang
benar untuk dipilih.

## 2. Tampilan sesudah perbaikan

```text
Pasien / Kunjungan *                                                        ▾
┌──────────────────────────────────────────────────────────────────────┐
│ [ Cari data...                                                      ] │
├──────────────────────────────────────────────────────────────────────┤
│ Jesse Pinkman · 00-00-00-18 · ENC-RSMMC-00175 · 16 Okt 2026 08.10 ·   │  ← terpilih
│   RANAP · Tunai                                                      │
│ Jesse Pinkman · 00-00-00-18 · ENC-RSMMC-00173 · 15 Okt 2026 14.05 ·   │
│   RANAP · Tunai · sudah ada invoice                                  │
└──────────────────────────────────────────────────────────────────────┘
```

(Nomor kunjungan dan tanggal di atas adalah contoh pseudonim mengikuti pola `ENC-RSMMC-xxxxx`
yang nyata di sistem, bukan data pasien sungguhan.)

| Keadaan | Perilaku |
| --- | --- |
| Memuat / kosong / gagal | Tidak berubah — perilaku `FilterSelect` (loading, empty, error) yang sudah ada tidak disentuh |
| `encounterNumber`/`encounterDate` kosong dari backend | Fallback `"-"` pada No. Kunjungan (pola fallback yang sama dengan field lain di fungsi ini); `formatDate(undefined)` mengembalikan `"-"` (perilaku existing `encounter-context-card.jsx`, tidak diubah) |
| Pencarian sisi server (`handleEncounterSearch`) | Tidak diubah — tetap mencari berdasarkan nama pasien, No. RM, atau No. Kunjungan |

## 3. Perubahan

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/health-services/billing-management/billing-invoices/create-manual/create-manual-invoice-view.jsx` | Baca `encounterNumber`/`encounterDate` dari tiap opsi (fallback camelCase/PascalCase, pola sama dengan field lain di fungsi `map` ini); format tanggal lewat `formatDate` yang diimpor dari `encounter-context-card.jsx`; sisipkan keduanya ke template string `label`, di antara No. RM dan jenis layanan. Tidak ada perubahan pada `value` opsi (`encounter.id`), logika `paymentDisplay`, atau tanda `hasInvoice` |
| `src/components/view/health-services/billing-management/billing-invoices/create-manual/encounter-context-card.jsx` | `formatDate` (fungsi lokal `dateStyle: "medium"`/`timeStyle: "short"`) diekspor (`export const`) supaya dipakai ulang oleh `create-manual-invoice-view.jsx` — satu sumber format tanggal/jam untuk layar ini, tidak ada formatter baru yang diduplikasi. Tidak ada perubahan perilaku pada komponen `EncounterContextCard` itu sendiri |

## 4. Verifikasi

| Perintah | Hasil |
| --- | --- |
| `npx eslint <2 berkas yang diubah> --quiet` | exit 0, 0 error |
| `npm run build` | **Lulus** (exit 0) — termasuk `prebuild`/`postbuild`, tidak ada galat kompilasi pada kedua berkas |
| Uji manual live browser (buka dropdown, pilih opsi, cek `EncounterContextCard`) | `NOT FEASIBLE` — backend (`NewQuilvianSystemBackend`, `dotnet run`) tidak dijalankan secara otomatis sesuai instruksi kerja eksplisit untuk repository ini; tanpa backend aktif, dropdown tidak bisa menembak data `encounter-options` sungguhan untuk diverifikasi visual di browser |
| Automated test | `SKIPPED (opsional)` — repository frontend tidak memakai Jest; tidak ada test unit existing yang mencakup `create-manual-invoice-view.jsx`; menulis test baru untuk perubahan string label satu baris ini tidak diminta dan di luar cakupan `FIX-BKC-003-01` |

**Verifikasi logika (pengganti sebagian uji manual yang terhalang):** perubahan murni
penggabungan string (`template literal`) dari field yang sudah tervalidasi tipenya (`string`,
`string | Date`) dan fallback `"-"` yang konsisten dengan pola existing di fungsi yang sama —
dibaca ulang pada diff final, tidak ditemukan jalur yang bisa melempar exception saat render
(`formatDate` sudah menangani `value` kosong/invalid dengan `return "-"` sebelum dipakai
`encounter-context-card.jsx` di produksi).

## 5. Acceptance criteria (`FIX-BKC-003-01`)

| AC | Bukti |
| --- | --- |
| 1. Untuk pasien dengan ≥2 kunjungan aktif berlabel sama sebelumnya, tiap baris kini punya No. Kunjungan berbeda | Benar secara konstruksi: `encounterNumber` unik per `RegPatientEncounter.Id` (field backend), selalu disisipkan ke `label` — dua kunjungan dengan `Id` berbeda tidak pernah lagi menghasilkan string `label` yang identik selama `EncounterNumber`-nya berbeda (selalu demikian, auto-increment per kunjungan) |
| 2. Seluruh info lama (nama, No. RM, jenis layanan, cara bayar, penjamin, tanda "sudah ada invoice") tetap tampil | Diff hanya menyisipkan 2 field baru di tengah template string; seluruh field lama dan urutan logikanya (`paymentDisplay`, `hasInvoice`) tidak disentuh |
| 3. Pencarian sisi server tetap berfungsi | `handleEncounterSearch`/`onSearchChange`/`getActiveEncounterOptions` tidak diubah sama sekali oleh task ini |
| 4. `EncounterContextCard` tetap konsisten | Komponen tidak diubah perilakunya — hanya fungsi internalnya (`formatDate`) ditambah kata kunci `export`, badan fungsi identik byte-per-byte |

## 6. Risiko

Label jadi lebih panjang — berpotensi terpotong di layar sempit bila `FilterSelect` tidak
membungkus teks panjang secara visual. Belum diverifikasi secara visual (lihat `NOT FEASIBLE` di
atas); disarankan pemeriksaan visual singkat begitu backend dapat dijalankan manual oleh pemilik
task, khususnya pada lebar form desktop yang biasa dipakai kasir. Tidak ada risiko pada data
tersimpan — `value` opsi yang dikirim ke server saat submit tidak berubah.
