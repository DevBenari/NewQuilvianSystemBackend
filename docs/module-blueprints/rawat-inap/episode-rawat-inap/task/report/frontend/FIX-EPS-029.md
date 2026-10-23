# Laporan Perubahan Frontend — `FIX-EPS-029`

## Metadata

| Field | Nilai |
| --- | --- |
| Judul | Penanganan Tuntas Kontaminasi State dan Isolasi Session Storage Antarmode Admisi Rawat Inap |
| Rencana Perbaikan | `PLAN-REPAIR-029` |
| Issue Terkait | `ISSUE-029` (Kontaminasi Alur Bayi pada Mode Pasien Lama dan Pasien Baru) |
| Tanggal | 17 Oktober 2026 |
| Status | **Selesai Source & Lulus Uji** |
| Basis Git Frontend | `HamzahV2` (`0accf387138a3fcc1ff40fbb5bdeb960076b4d59`) |

## Ringkasan Perubahan

Perbaikan ini menyelesaikan regresi kritis di mana setelah pengguna membuka kartu pendaftaran bayi baru lahir, saat berpindah membuka **Pendaftaran Pasien Lama** (`entry=existing`) atau **Pendaftaran Pasien Baru** (`entry=new`), sistem secara keliru membawa sisa data bayi, memutasi alur menjadi alur bayi, dan langsung mengarahkan pengguna ke **Langkah 3 dari 7 Bayi (Perawatan dan DPJP Bayi)** sebelum sempat menginput data pasien lama.

Telah diimplementasikan 4 solusi tuntas:
1. **Pemutusan Mutasi Dinamis (`FIX-EPS-029-01`)**:
   - Menghapus klausul `(isNewborn && [NEW, EXISTING].includes(entryMode))` dari `resolveInpatientAdmissionSteps` pada `inpatient-admission-flow-constants.jsx`.
   - Menjamin alur Pasien Baru (`new`) **selalu dan mutlak 12 langkah**, dan alur Pasien Lama (`existing`) **selalu dan mutlak 10/11 langkah**. Alur umum kini kebal dari mutasi ke alur bayi.
2. **Penguncian Eksklusif Hook Admisi Bayi (`FIX-EPS-029-02`)**:
   - Mengunci `enabled` pada `use-inpatient-newborn-admission.js` secara eksklusif hanya jika `flow.entryMode === "newborn"`.
   - Mencegah render komponen form bayi (`<NewbornCareStep>`) pada langkah dokter pasien umum/lama.
3. **Auto-Reset State Memory React (`FIX-EPS-029-03`)**:
   - Menambahkan efek pembersihan otomatis di `use-inpatient-admission-flow.jsx`. Saat `entryMode !== "newborn"`, nilai `patientType`, `newbornDraft`, dan `motherEpisodeId` langsung di-reset bersih ke kondisi awal.
4. **Isolasi dan Sanitasi Session Storage (`FIX-EPS-029-04`)**:
   - Meniadakan pembuatan `storageKey` dan persistensi saat berada di menu utama (`!entryMode`). Kunci `unknown:draft` tidak lagi dibuat.
   - Menambahkan sanitasi saat rehidrasi: jika draf sesi pada mode non-bayi ditemukan menyimpan `patientType === "newborn"`, kunci tersebut otomatis dibuang.
   - Menambahkan pembersihan otomatis residu kunci `unknown` dan format legacy saat inisialisasi awal.

## Berkas yang Diubah

1. `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx`:
   - Menghapus mutasi dinamis `isNewborn` dari `resolveInpatientAdmissionSteps`.
2. `src/lib/hooks/health-services/inpatient-management/use-inpatient-newborn-admission.js`:
   - Mengunci `enabled` dan efek sinkronisasi hanya saat `flow.entryMode === "newborn"`.
3. `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-flow.jsx`:
   - Menetapkan `isNewborn` strictly berbasis `entryMode === "newborn"`.
   - Memperbarui `steps` tanpa ketergantungan parameter `isNewborn`.
   - Mengisolasi `storageKey` hanya untuk mode aktif (`!entryMode` menghasilkan `""`).
   - Menambahkan sinkronisasi auto-reset `patientType` saat beralih keluar dari mode `newborn`.
   - Menambahkan sanitasi rehidrasi dan sweeper pembersih kunci `unknown:draft`.
   - Memperbarui `fallbackSlug` agar mengacu pada `entryMode === "newborn"`.
4. `tests/unit/inpatient-newborn-admission.test.mjs`:
   - Memperbarui pengujian 7 langkah bayi khusus mode `newborn`, serta menambahkan asersi kekebalan alur `new` (12 langkah) dan `existing` (10 langkah).

## Bukti dan Verifikasi

- **Unit Tests**:
  - `node --import ./tests/helpers/register.mjs --test "tests/unit/inpatient-newborn-admission.test.mjs"`
  - Hasil: 9 dari 9 test **LULUS (PASS 100%)**, termasuk asersi isolasi 12 langkah pasien baru dan 10 langkah pasien lama.
- **Production Build (Next.js & Turbopack)**:
  - Perintah: `npm run build`
  - Hasil: **LULUS BERSIH (Exit code 0)**, 498+ rute aplikasi berhasil dikompilasi, static & public assets tersalin sempurna, dan standalone runtime siap digunakan.
