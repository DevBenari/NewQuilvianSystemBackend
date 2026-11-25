# Laporan Perubahan Frontend — `FE-HRD-R22`

## Metadata

| Field | Nilai |
| --- | --- |
| Task / judul | `FE-HRD-R22` — Perjalanan Dinas bagian 1: dasbor, tarif, SPPD |
| Slice / trace | `S-D5`; `FE-HRD-292`–`297` |
| Kontrak / dependency | API Contract v23 bagian 29 (`approved`); `FE-HRD-R09` ✅ |
| Commit | FE `ec3616405`; BE `7b2b15ccd` (read-only) |
| Tanggal / status | 16 November 2026 / 🟡 **SEBAGIAN** |

## Hasil

Dasbor, formulir SPPD, daftar/detail, tarif/zona, dan kalkulator uang harian menggunakan `BusinessTravelService` nyata. Estimasi uang harian berasal dari tarif zona server, sedangkan nilai final dihitung ulang server saat pembuatan. Pembatalan memanggil endpoint `PUT requests/{id}/cancel` dengan alasan minimal 15 karakter. Tidak ada tarif atau permohonan contoh.

API belum memiliki operasi ubah/nonaktifkan tarif; layar hanya menawarkan tambah tarif dan menyatakan batas kontrak tersebut.

## UI gate dan verifikasi

`UI GATE: PASS — 5 berkas target, 0 raw form/table/button, 0 Tailwind; base component dan CSS Module bertoken.`

| Pemeriksaan | Hasil |
| --- | --- |
| `npm run lint` | PASS — 0 errors; 1252 warning existing |
| `npm run build` | PASS — 814 static pages |
| Zona/tarif/kalkulator | API nyata; uang harian final dihitung server |
| Buat/daftar/detail/batal SPPD | API nyata dan cocok dengan controller backend |
| Browser terautentikasi | Belum — `/api/v1/auth/me` lokal 401 |

Acceptance terpenuhi secara source. Uji submit/batal dengan data non-produksi dan permission berbeda belum dapat dilakukan, sehingga status tetap 🟡. Perubahan sampingan: `NONE`.
