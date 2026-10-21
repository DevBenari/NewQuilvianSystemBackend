# Laporan Task Frontend — `FE-RJP-002` Label member di Pendaftaran Pasien

Definisi task: [priority-reservation-frontend-roadmap.md](../../../roadmap/priority-reservation-frontend-roadmap.md) bagian `FE-RJP-002`.

## 1. Ringkasan

| Field | Isi |
| --- | --- |
| Task ID | `FE-RJP-002` |
| Slice | `MVP-0`, `EPIC RP-08` |
| Trace | `FR-RP-070`; PDF poin 2C; `RJ-PRI-FE-004` (`DEV_DISCRETION` bentuk label); `R-RP-28`, `UAT-RP-18`, `UAT-RP-19` |
| Kontrak | Respons pencarian pasien yang sudah ada (`CAP-RP-21`): `isMember`, `defaultMembershipTierName` |
| Dependency | — |
| Task mode | `FRONTEND` (`CROSS-REPO MODE`, `RJ-PRI-DEC-031`) |
| Branch / baseline | FE `sukmagpV2` @ `17e0ffec0`, working tree belum di-commit |
| Tanggal | 16 Oktober 2026 |
| Model | Claude Opus 5 |
| Status | ✅ Selesai — uji layar 4/4 `PASS`, lint dan build `PASS`. Deviasi cakupan dicatat di bagian 6 |

## 2. Proses bisnis

Petugas admisi mencari pasien lama di **Rawat Jalan → Pendaftaran Pasien**. Pada langkah *Verifikasi Data Pasien*,
panel identitas di kiri kini menampilkan label tingkat member bila pasien member Aktif. Petugas langsung tahu pasien
itu prioritas dan mengarahkannya ke ruang tunggu prioritas (PDF 2C).

| Keadaan pasien | Label |
| --- | --- |
| Member Aktif, tingkat "Gold Member" | `Gold Member` |
| Member Aktif, nama tingkat tanpa kata "Member" (mis. "Gold") | `Member Gold` |
| Member Aktif, nama tingkat kosong | `Member` |
| Reguler, Menunggu Pembayaran, atau Kedaluwarsa | Tanpa label |

Label hanya tampilan. Status member ditentukan backend (resolver CRM menulis proyeksi `isMember` dan
`defaultMembershipTierName`). Frontend tidak menghitung status member sendiri.

## 3. Perubahan

| Berkas (repo frontend) | Status | Isi |
| --- | --- | --- |
| `src/utils/health-services/registration-management/outpatient-registration/outpatient-registration.utils.js` | Diperbarui | `getOutpatientMembershipLabel(patient)` — membaca `isMember`/`IsMember` dan `defaultMembershipTierName` (termasuk dari `raw`); nama tingkat yang sudah memuat kata "Member" tidak digandakan |
| `src/components/view/health-services/registration-management/outpatient-registration/outpatient-identity-panel.jsx` | Diperbarui | `StatusBadge status="info"` di bagian atas panel identitas bila label ada |
| `tests/unit/outpatient-membership-label.test.mjs` | Baru | 5 uji pemetaan label |

Temuan saat uji layar: nama tingkat di master data sudah berbentuk "Gold Member", "Silver Member", dan seterusnya.
Versi pertama menghasilkan "Member Gold Member". Fungsi diperbaiki agar nama tingkat yang sudah memuat kata "Member"
dipakai apa adanya, lalu uji unit dan uji layar dijalankan ulang.

## 4. Gerbang komponen dasar

| Elemen | Keputusan | Bukti |
| --- | --- | --- |
| Label tingkat member | `REUSE` `StatusBadge` (`status="info"`, `showIcon={false}`) | `src/components/features/base-features/status-badge.jsx`; dipakai di tabel master data (mis. `bank-table-columns.jsx`, `kiosk-device-table-columns.jsx`) |
| Panel identitas | `REUSE` — tidak diubah selain menyisipkan label | `outpatient-identity-panel.jsx` |

`UI GATE: PASS — semua elemen REUSE; tidak ada NEW atau EXTEND yang menunggu keputusan user.`

Konsistensi visual: tidak ada CSS baru dan tidak ada nilai warna, jarak, atau huruf yang ditulis langsung. Warna label
berasal dari kelas `region-status-info` milik `StatusBadge`.

## 5. Validasi

| Langkah | Hasil |
| --- | --- |
| `npm run lint:errors` | exit 0 |
| `npm run build` | exit 0 (Next.js 16.2.12, standalone siap) |
| `AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/outpatient-membership-label.test.mjs` | `PASS` 5/5 |
| `AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/*.test.mjs` | 2969/2996; 27 gagal di modul lain, tidak ada yang mengimpor berkas task ini |
| `MANUAL TEST` (Playwright, build standalone port 3000, backend uji 7185) | `PASS` 4/4, rincian di bawah |

### Uji layar

Data samaran: pasien `RJPF UJI GOLD ...` dengan membership Gold Aktif, dan pasien `RJPF UJI REG ...` dengan membership
Gold berstatus Menunggu Pembayaran. Keduanya disisipkan lewat SQL data uji. Proyeksi `IsMember` pada pasien Gold diisi
SQL dengan nilai yang sama dengan hasil `PatientMembershipStatusResolver.SyncPatientProjectionAsync`. Pasien Menunggu
Pembayaran tetap `IsMember = false`, sesuai aturan CRM-DEC-004. Pencarian memakai No. Rekam Medis.

| ID | Pemeriksaan | Hasil |
| --- | --- | --- |
| F2-1 | **`R-RP-28`, `UAT-RP-18`**: pasien Gold Aktif, label `Gold Member` tampil di panel identitas | `PASS` |
| F2-1b | Tidak ada label ganda `Member Gold Member` | `PASS` |
| F2-2 | **`UAT-RP-19`**: pasien Gold Menunggu Pembayaran, tanpa label member | `PASS` |
| F2-3 | Lebar layar 375 px: label `Gold Member` tetap tampil di atas panel identitas | `PASS` |

Tangkapan layar: `fe-rjp-002-gold.png` dan `fe-rjp-002-gold-375.png` di scratchpad sesi.

## 6. Deviasi cakupan

Kartu task menyebut label di **hasil pencarian** dan **panel identitas**. Yang dikerjakan hanya panel identitas.

Alasannya: daftar hasil pencarian dirender oleh `emergency-registration/patient-selection-step.jsx`, komponen bersama
IGD dan Rawat Jalan. Berkas itu sedang berisi suntingan user yang belum di-commit, dan sesi ini dilarang menyentuhnya.

Dampaknya kecil. Pencarian Rawat Jalan memakai No. RM, NIK, atau No. HP, sehingga satu pencarian biasanya langsung
membuka langkah *Verifikasi Data Pasien* tempat label tampil. Petugas tetap melihat status member sebelum melanjutkan
pendaftaran.

Usulan tindak lanjut: sesudah suntingan user di `patient-selection-step.jsx` di-commit, tambahkan label yang sama di
kartu kandidat hasil pencarian memakai `getOutpatientMembershipLabel` dan `StatusBadge`.

## 7. Wewenang dan batas

- Bentuk label adalah `DEV_DISCRETION` (`RJ-PRI-FE-004`). Dipilih `StatusBadge` info agar seragam dengan label status di
  layar lain.
- Risiko yang diterima di kartu task: proyeksi bisa tertinggal antara 00.00 dan 00.05 WIB. Untuk tampilan, hal ini
  dapat diterima. Klasifikasi antrean tetap memakai resolver di backend (`BE-RJP-002`), bukan label ini.
- Suntingan user yang belum di-commit di `emergency-registration/patient-selection-step.jsx` tidak disentuh.
- Tidak ada stage, commit, push, merge, atau deploy.
