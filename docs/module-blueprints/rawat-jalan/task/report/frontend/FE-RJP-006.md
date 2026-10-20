# Laporan Task Frontend — `FE-RJP-006` Pendaftaran Pasien mode reservasi

Definisi task: [priority-reservation-frontend-roadmap.md](../../../roadmap/priority-reservation-frontend-roadmap.md) bagian `FE-RJP-006`.

## 1. Ringkasan

| Field | Isi |
| --- | --- |
| Task ID | `FE-RJP-006` |
| Slice | `MVP-1`, `EPIC RP-05` |
| Trace | `FR-RP-040`, `061`; PDF poin 2B; `RJ-PRI-DEC-017`, `030` usulan (1); `VAL-RP-17`; `UAT-RP-11`, `UAT-RP-12` |
| Kontrak | `RJ-PRI-RESV-001@1.0.0`: `POST /health-services/registration-management/patient-encounters` dengan `priorityReservationId`; respons `queueClassification.priorityWaitingLocationName`; `GET /outpatient-priority-reservations/{id}` |
| Dependency | `FE-RJP-004` ✅, `BE-RJP-008` ✅, `BE-RJP-011` ✅ |
| Task mode | `FRONTEND` (`CROSS-REPO MODE`, `RJ-PRI-DEC-031`) |
| Branch / baseline | FE `sukmagpV2` @ `17e0ffec0`, working tree belum di-commit |
| Tanggal | 19 Oktober 2026 |
| Model | Claude Opus 5 |
| Status | ✅ Selesai — uji layar 7/7 `PASS` termasuk regresi pendaftaran biasa, lint dan build `PASS` |

## 2. Proses bisnis

Pasien reservasi sudah tiba dan sudah diskrining. Petugas admisi menekan **Lengkapi Pendaftaran** di daftar atau
detail reservasi. Wizard **Pendaftaran Pasien** terbuka dengan alamat
`/health-services/registration-management/outpatient-registration?priorityReservationId=<id>`.

| Keadaan reservasi | Yang terjadi di wizard |
| --- | --- |
| Sudah Tiba, tanggal hari ini | Banner "Melengkapi reservasi RSV-…". Pasien dipilih otomatis dan wizard langsung ke langkah **Jenis Kunjungan**. Tanggal kunjungan terkunci ke hari ini. Petugas memilih poli, dokter, dan penjamin seperti biasa |
| Status lain atau tanggal lain | Peringatan "Reservasi tidak dapat dilengkapi", mis. "Reservasi RSV-20261019-0006 sedang berstatus Menunggu Kedatangan untuk 19 Oktober 2026. Pendaftaran dari reservasi hanya untuk reservasi Sudah Tiba pada hari ini." Tombol **Kembali ke Reservasi Prioritas**. Wizard tidak tampil |

Saat **Selesaikan Pendaftaran**, frontend mengirim `priorityReservationId`. Backend membuat kunjungan dan mengubah
reservasi menjadi **Menjadi Kunjungan** dalam satu transaksi. Layar selesai menulis
"Arahkan pasien ke Ruang Tunggu Prioritas. Petugas akan menghampiri pasien di sana." Tombol **Daftarkan Pasien Lain**
keluar dari mode reservasi (parameter dihapus dari alamat) sehingga pendaftaran berikutnya kembali normal.

Tombol Kembali pada langkah Jenis Kunjungan di mode reservasi membawa petugas ke detail reservasi, bukan ke pilihan
Pasien Lama/Baru, karena pasien tidak boleh diganti.

## 3. Perubahan

| Berkas (repo frontend) | Status | Isi |
| --- | --- | --- |
| `src/app/health-services/registration-management/outpatient-registration/page.jsx` | Diperbarui | Membaca `searchParams.priorityReservationId` di batas route (tanpa `useSearchParams`/Suspense) |
| `src/components/view/health-services/registration-management/outpatient-registration/outpatient-registration-page.jsx` | Diperbarui | Prop `priorityReservationId`, panel memuat/tidak dapat dilengkapi, banner mode reservasi, Kembali ke detail, `visitDateLocked`, keluar mode saat reset |
| `src/lib/hooks/health-services/registration-management/outpatient-registration/use-outpatient-priority-reservation-mode.js` | Baru | Muat detail, cek Sudah Tiba + hari ini, pilih pasien lewat pencarian Pendaftaran yang sudah ada, kembali/keluar mode |
| `src/lib/hooks/health-services/registration-management/outpatient-registration/use-outpatient-registration.js` | Diperbarui | Payload menyertakan `priorityReservationId` hanya bila ada |
| `src/components/view/health-services/registration-management/outpatient-registration/outpatient-visit-step.jsx` | Diperbarui | `visitDateLocked` menonaktifkan isian tanggal |
| `src/components/view/health-services/registration-management/outpatient-registration/outpatient-completed-step.jsx` | Diperbarui | Kalimat arahan ke ruang tunggu prioritas bila respons memuat `priorityWaitingLocationName` |

## 4. Gerbang komponen dasar

Modul referensi visual: wizard Pendaftaran Pasien itu sendiri. Tidak ada rupa baru.

| Elemen | Keputusan | Bukti |
| --- | --- | --- |
| Panel memuat / tidak dapat dilengkapi, banner mode reservasi | `REUSE` `EmergencyInlineAlert` (komponen alert yang sudah dipakai wizard) | `outpatient-registration-page.jsx`, `emergency-registration/*` |
| Tombol Kembali ke Reservasi Prioritas | `REUSE` `BaseButton` | `base-features/base-button` |
| Tanggal kunjungan terkunci | `REUSE` `EmergencyDateField` dengan prop `disabled` yang sudah ada | `outpatient-visit-step.jsx` |
| Kalimat arahan di layar selesai | `REUSE` teks yang sudah ada, kalimat berganti sesuai data | `outpatient-completed-step.jsx` |

`UI GATE: PASS — semua elemen REUSE; tidak ada NEW atau EXTEND.`

Checklist konsistensi UI: tidak ada CSS baru, warna literal, `<button>` mentah baru, atau inline style. Grep pada
baris yang ditambahkan tidak menemukan `<button`, `style={{`, atau nilai hex.

## 5. Validasi

| Langkah | Hasil |
| --- | --- |
| `npm run lint:errors` | exit 0 |
| `npm run build` | exit 0 (build akhir 19 Oktober 2026) |
| `AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/*.test.mjs` | 2976/3003; 27 gagal di modul lain, sudah gagal pada snapshot `HEAD`. Tidak ada uji unit baru untuk task ini (opsional) |
| `MANUAL TEST` (Playwright, build standalone port 3000, backend uji 7185) | `PASS` 7/7, rincian di bawah |

### Uji layar

Data samaran: reservasi `resDetail` (Menunggu Kedatangan) dan `resLink` (Sudah Tiba lewat konfirmasi link, hari ini).

| ID | Pemeriksaan | Hasil |
| --- | --- | --- |
| **`UAT-RP-12`** | Reservasi Menunggu Kedatangan: pesan status + tombol kembali; wizard (Pasien Lama/Baru) tidak tampil | `PASS` |
| BACK | Tombol kembali membuka detail reservasi yang sama | `PASS` |
| LOCKED patient | Mode reservasi: banner tampil, pasien terpilih otomatis, langsung di Jenis Kunjungan | `PASS` |
| LOCKED date | Isian tanggal kunjungan nonaktif (hari ini) | `PASS` |
| **`UAT-RP-11`** | Selesai: nomor antrean terbit (`I100`), arahan "Arahkan pasien ke Ruang Tunggu Prioritas", reservasi berstatus Menjadi Kunjungan (`status=3`) | `PASS` |
| EXIT mode | "Daftarkan Pasien Lain": parameter hilang dari alamat, pilihan Pasien Lama/Baru tampil | `PASS` |
| REGRESSION | Tanpa parameter: wizard biasa tanpa banner reservasi | `PASS` |

Nomor antrean kelipatan 5 dan transaksi kunjungan + reservasi dibuktikan di backend (`BE-RJP-008`, laporan backend).
Nomor `I100` pada uji ini mengikuti aturan itu.

Tangkapan layar: `fe-rjp-006-invalid.png`, `fe-rjp-006-visit.png`, `fe-rjp-006-completed.png` di scratchpad sesi.

## 6. Delta dan risiko

| Butir | Catatan |
| --- | --- |
| Pemilihan pasien | Pasien diambil lewat `searchEmergencyRegistrationPatients` dengan No. RM reservasi, lalu dicocokkan `patientId`. Bentuk data sama dengan pilihan petugas biasa (panel identitas, label member, penjamin) |
| Batas hari ini | Frontend menolak lebih awal reservasi yang bukan Sudah Tiba hari ini. Backend tetap menegakkan aturan yang sama (`VAL-RP-17`) |
| Risiko | Wizard dipakai setiap hari. Regresi pendaftaran biasa diuji dan lulus. Semua perubahan aktif hanya bila `priorityReservationId` ada |

## 7. Wewenang dan batas

- Suntingan user yang belum di-commit di `emergency-registration/patient-selection-step.jsx` tidak disentuh.
- Tidak ada stage, commit, push, merge, atau deploy.
