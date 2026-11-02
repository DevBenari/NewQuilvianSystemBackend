# Laporan Perubahan Frontend — `FE-MCU-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-MCU-005` |
| Judul | Layar jadwal, kuota, pelampauan, dan dokter jaga |
| Slice | `MVP-0` (pasangan `BE-MCU-007` ✅) |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian *`FE-MCU-005`* |
| Trace | `FR-MCU-005`, `006`; `MCU-DEC-042` (dokter jaga harian); brief layar [`05-screen-brief.md`](../../../05-screen-brief.md) bagian 1 no. 4, 2, 3, 5.9 |
| Contract version | `MCU-API-v1` grup `Mcu Daily Schedule` — `approved` 2026-10-21; `VAL-MCU-13`, `14`. Tabel kontrak masih bertanda *Rencana*, tetapi ketiga endpoint sudah dilayani `McuScheduleController` (`BE-MCU-007` ✅) dengan route, method, dan DTO yang sama |
| Wewenang UI | Brief layar `FE-MCU-000` ✅ (keputusan pemilik 2026-10-21); sumber pilihan dokter jaga dan perusahaan penjamin: keputusan pemilik 2026-10-27 (lihat §1) |
| Dependency | `FE-MCU-000` ✅, `FE-MCU-002` ✅, `BE-MCU-007` ✅ |
| Klasifikasi | `MEDIUM` — satu repository, ±7 berkas baru + 6 berkas diubah, satu layar dengan dua modal, kontrak yang sudah ada, tanpa database, keamanan berkaitan (izin per aksi `McuSchedule`, sumber opsi berizin), UI sedang |
| Task mode | `FRONTEND` |
| Target tulis | Frontend: route, view, hook, utilitas, konstanta jadwal, menu, satu baris slice jadwal, uji unit + E2E; revisi kecil `FE-MCU-003` (sumber perusahaan penjamin) dan `FE-MCU-004` (keterangan modal). Backend: laporan ini, revisi laporan `FE-MCU-003`/`004`, tanda roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `d0888bf6f` (branch `YogaV2`) |
| Commit backend yang dijadikan rujukan | `a3b63b50` (branch `yoga`) |
| Tanggal | 2026-10-27 |
| Status | Selesai — ketiga kriteria terbukti di E2E API-tiruan; UAT baca terhadap backend lokal lulus |

---

## 1. Keadaan yang ditemukan di awal

- Lapisan data `FE-MCU-002` ✅ sudah punya 3 thunk grup `Mcu Daily Schedule` di slice `mcuSchedule`.
- **Cacat kecil di slice itu:** `fetchMcuDailySchedules.rejected` tidak mengabaikan permintaan yang dibatalkan,
  padahal slice paket dan referensi sudah melakukannya. Akibatnya, saat bulan diganti cepat, galat *canceled* bisa
  tertinggal di layar karena `fulfilled` tidak menghapus galat.
- Backend (`McuScheduleService`):
  - `GET` mengembalikan setiap tanggal dalam rentang, termasuk tanggal yang belum diatur (`isConfigured: false`,
    dianggap tutup);
  - rentang paling panjang 92 hari; tanggal lewat ditolak;
  - kuota di bawah peserta terjadwal → `409` `VAL-MCU-14`; tanggal yang sudah berisi peserta tidak dapat ditutup;
  - `dutyMcuDoctorId` kosong = tanpa dokter jaga untuk seluruh rentang;
  - pelampauan: jumlah ≥ 1, alasan wajib `VAL-MCU-13` (≤ 500), hanya untuk tanggal buka yang belum lewat.
- Kontrak tidak menetapkan sumber pilihan dokter jaga.
  - `/doctors/options` dijaga policy `KioskRead`, yang hanya meloloskan SuperAdmin/Administrator/akun kiosk.
  - `/doctors/admin/options` dijaga `Doctor : Read`.
  - Masalah yang sama ditemukan pada pilihan perusahaan penjamin layar paket `FE-MCU-003`
    (`/company-guarantors/options`).
  - **Keputusan pemilik (2026-10-27):** pakai `/admin/options` untuk keduanya, dan perbaiki layar paket sekalian.
- devYoga belum punya jadwal MCU yang diatur (seluruh Oktober *Tutup*).

---

## 2. Proses bisnis dari sisi pengguna

1. **Menu** *Medical Check-Up › Jadwal & Kuota* (`McuSchedule : Read`), di atas sub-grup *Master Data*.
2. **Tabel per tanggal dalam satu bulan** (keputusan brief: tabel, bukan kalender).
   - Kolom: Tanggal (*Sel 27-10*), Buka (*Ya*/*Tutup*), Kuota, Terpakai, **Sisa Kuota** (*26/30* = sisa/kapasitas,
     kapasitas termasuk pelampauan), Pelampauan (*+5* atau *—*), Dokter Jaga (*—* bila kosong).
   - Tanggal yang belum diatur tampil *Tutup* dengan nilai *—*.
   - Bulan dipilih dari daftar (2 bulan ke belakang sampai 11 bulan ke depan); tombol reset kembali ke bulan berjalan.
3. **Atur Rentang Tanggal** (tombol hanya bagi `McuSchedule : Update`) membuka modal berisi:
   - dari–sampai (kalender, tanggal lewat dinonaktifkan);
   - sakelar *Buka*, kuota per tanggal, dan dokter MCU jaga (opsional, *Tanpa dokter jaga*);
   - keterangan bahwa pengaturan menimpa seluruh tanggal di rentang, termasuk dokter jaga.

   Rentang, tanggal lewat, dan kuota kosong untuk hari buka diperiksa di layar dengan kalimat server. **Kuota di bawah
   terpakai tidak diperiksa layar**: server menolak dengan `409`, kalimatnya tampil apa adanya di modal, dan isian
   tetap. Sesudah berhasil, baris tanggal terkait diganti dari jawaban server tanpa memuat ulang bulan.
4. **Setujui Pelampauan** — kolom *Aksi* dan tombolnya hanya dirender bagi `McuSchedule : ApproveQuotaOverride`,
   dan hanya pada tanggal buka yang belum lewat. Modal memuat:
   - ringkasan *kuota, terpakai, sisa*;
   - tambahan kuota dan alasan (wajib);
   - daftar pelampauan sebelumnya beserta alasannya.

   Penolakan server tampil di modal apa adanya.
5. **Gagal muat** → pesan + *Coba lagi*; **`403`** → halaman akses ditolak.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah (frontend)

| Berkas | Perubahan |
| --- | --- |
| `src/app/health-services/medical-checkup-management/schedules/page.jsx`, `schedules-client.jsx` (baru) | Route `…/schedules` |
| `src/components/view/…/medical-checkup-management/schedules/mcu-schedule-view.jsx` (baru) | Tabel per tanggal, modal *Atur Rentang Tanggal* dan *Setujui Pelampauan* |
| `src/lib/hooks/…/medical-checkup-management/schedules/use-mcu-schedule.jsx` (baru) | Bulan, muat jadwal, dua modal, pilihan dokter jaga |
| `src/utils/…/medical-checkup-management/schedules/mcu-schedule-utils.js` (baru) | Bulan, label hari, tampilan baris, aturan tombol pelampauan, form/validasi/payload kedua modal |
| `src/lib/constants/…/medical-checkup-management/schedules/mcu-schedule-constants.js` (baru) | Config, batas (92 hari, alasan 500), resource dokter jaga `/doctors/admin/options` |
| `src/lib/state/slice/…/mcu-schedule-slice.jsx` | `fetchMcuDailySchedules.rejected` mengabaikan permintaan yang dibatalkan (sama dengan slice MCU lain) |
| `src/utils/menu-sidebar/menu-items.jsx` | Butir *Jadwal & Kuota* (`RiCalendarCheckLine` yang sudah diimpor) |
| `src/lib/constants/…/mcu-packages/mcu-package-constants.js`, `src/lib/hooks/…/mcu-packages/use-master-data-mcu-package-company-prices.jsx` | **Revisi `FE-MCU-003`:** perusahaan penjamin dari `/company-guarantors/admin/options` |
| `src/components/view/…/mcu-fitness-categories/master-data-mcu-fitness-category-view.jsx` | **Revisi `FE-MCU-004`:** keterangan modal dipindah ke isi modal |
| `tests/unit/mcu-schedule-screen-rules.test.mjs` (baru) | 7 uji (aturan layar + slice) |
| `tests/e2e/mcu-schedule-screen.spec.mjs` (baru) | 6 skenario API-tiruan |
| `tests/e2e/mcu-package-screen.spec.mjs` | Tiruan perusahaan penjamin pindah ke path `admin/options` |

### 3.2 Kepatuhan arsitektur frontend

- Alur `view → hook → slice (FE-MCU-002) → service → InstanceAxios`.
- Komponen yang sudah ada: `DataFilter`, `FilterSelect` (bulan), `DataTable`, `ConfirmModal` berisi
  `BaseDateField`/`BaseSwitchField`/`BaseTextField`/`BaseSelectField`/`BaseTextAreaField`, `StatusBadge`, `ToastStack`.
- Sumber opsi berizin memakai objek resource `useSelectResource`, mengikuti preseden Bank Darah
  (`BDRS_DOCTOR_SELECT_RESOURCE`) dan admisi Rawat Inap.
- Tidak ada komponen, CSS Module, dependency, atau abstraksi baru.

---

## 4. State yang ditangani di layar

| State | Perilaku |
| --- | --- |
| Memuat | Penanda muat tabel; tombol dan modal terkunci saat mengirim |
| Kosong | Tidak terjadi untuk bulan sah, karena server selalu mengembalikan setiap tanggal; teks *Belum ada tanggal pada bulan ini.* disiapkan |
| Gagal muat | Pesan server + *Coba lagi*; pembatalan saat ganti bulan tidak dianggap gagal |
| Ditolak server | Kalimat server apa adanya di dalam modal; isian tetap |
| Tanpa hak akses | `403` → halaman akses ditolak; tanpa `Update` → tanpa *Atur Rentang Tanggal*; tanpa `ApproveQuotaOverride` → tanpa kolom *Aksi* |
| Tanggal lewat / tutup | Tanpa tombol pelampauan; kalender modal menonaktifkan tanggal lewat |

---

## 5. Endpoint yang dikonsumsi

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/medical-checkup-management/daily-schedules?from&to` | Satu bulan | `McuSchedule : Read` |
| `PUT` | `/v1/health-services/medical-checkup-management/daily-schedules` | Atur rentang | `McuSchedule : Update` |
| `POST` | `/v1/health-services/medical-checkup-management/daily-schedules/{scheduleDate}/quota-overrides` | Setujui pelampauan | `McuSchedule : ApproveQuotaOverride` |
| `GET` | `/v1/corporate/human-resource/master-data/doctors/admin/options` | Pilihan dokter jaga | `Doctor : Read` |
| `GET` | `/v1/administrator/master-data/company-guarantors/admin/options` | Pilihan perusahaan (layar paket, revisi) | `CompanyGuarantor : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `tests/unit/mcu-schedule-screen-rules.test.mjs` | 7/7 lulus | `PASS` | `# pass 7`, `# fail 0` |
| Cacat tertangkap uji unit | `formatMcuScheduleDay` salah mengambil grup regex (*"Sel 10-2026"*); diperbaiki sebelum E2E | `PASS` | Keluaran `node --test` |
| Uji mutasi slice | Baris pengabaian pembatalan dihapus → uji slice **gagal**; dipulihkan identik (`cmp`) | `PASS` | Keluaran `node --test` |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 3.629 uji: 3.583 lulus, 46 gagal — daftar nama kegagalan **identik** dengan baseline (semua di luar MCU) | `EXISTING / ENVIRONMENT ISSUE` | `diff` kosong |
| `npm run lint:errors` | Exit 0 | `PASS` | Keluaran perintah |
| `npm run build` | Exit 0: *Compiled successfully in 63s*, 523 halaman; route `…/schedules` terbentuk | `PASS` | Keluaran build |
| E2E API-tiruan `tests/e2e/mcu-schedule-screen.spec.mjs` | 6/6 lulus, tanpa yang dilewati | `PASS` | `6 passed (8.2s)` |
| **Kriteria 1** — sisa kuota per tanggal | Satu bulan satu permintaan (`from`/`to` awal–akhir bulan); baris besok *Ya · 30 · 4 · 26/30 · — · —*; hari ini *1/35 · +5 · dr. Rina*; belum diatur *Tutup · —*; ganti bulan dan reset mengirim rentang yang benar | `PASS` | Skenario *tabel* |
| **Kriteria 2** — kuota di bawah terpakai → `409` | Kuota kosong ditolak layar tanpa permintaan. Kuota 3 untuk rentang hari ini–akhir bulan dengan dr. Budi → body persis; `409` *"…Sudah ada 4 peserta terjadwal; kuota tidak boleh di bawah 4."* tampil apa adanya; isian tetap. Kuota 40 → tersimpan, baris diganti (*36/40 · dr. Budi*) tanpa GET ulang | `PASS` | Skenario *atur rentang* |
| **Kriteria 3** — tombol pelampauan hanya bagi `ApproveQuotaOverride` | Kepala unit: tombol pada tanggal buka hari ini/besok, tidak pada kemarin dan tanggal belum diatur. Pemegang `Update` saja: tanpa tombol dan tanpa kolom *Aksi*. Baca saja: tanpa *Atur Rentang Tanggal* | `PASS` | Skenario *tabel*, *pelampauan* ×2 |
| Pelampauan | Wajib isi di layar (kalimat server, tanpa permintaan); body persis; baris menjadi *4/38 · +8*; `409` server tampil di modal dengan alasan tetap | `PASS` | Skenario *pelampauan* ×2 |
| Gagal muat / `403` / menu | *Coba lagi* memulihkan; `403` → akses ditolak; menu tampil bagi `McuSchedule : Read` tanpa sub-grup Master Data | `PASS` | 2 skenario |
| Regresi `mcu-package-screen` + `mcu-reference-data-screen` | 21/21 lulus; uji harga perusahaan hanya meniru path `admin/options`, sehingga lulusnya membuktikan path baru dipakai | `PASS` | `21 passed` |
| UAT baca, backend lokal + devYoga (semua tulis diblokir) | Oktober 31 tanggal termuat (semua *Tutup*, belum diatur); modal rentang memuat dokter dari `doctors/admin/options` (`200`); modal harga perusahaan paket `UJI-MCU-BASIC` memuat 5 perusahaan dari `company-guarantors/admin/options` (`200`). Nol tulis, nol GET gagal | `PASS` | Log skrip + tangkapan layar |

`MANUAL TEST: PASS` — pemilih bulan + reset, kalender dari–sampai, sakelar buka, select dokter (opsi *Tanpa dokter
jaga*), kedua modal (validasi, penolakan, berhasil), dan visibilitas tombol per izin dijalankan lewat peramban
(Playwright) pada E2E dan UAT baca.
`AUTOMATED TEST: npm run test:unit — FAIL (46 kegagalan existing di luar MCU, identik dengan baseline; 7/7 uji baru PASS)`.

**Catatan uji:** membekukan jam peramban (`page.clock`) bertabrakan dengan pengelola sesi aplikasi, sehingga muat
ulang berakhir di halaman login. Spec karena itu menghitung data tiruan relatif terhadap tanggal WIB hari ini. Di
tanggal tepi bulan, uji yang butuh kemarin/besok/lusa dalam bulan yang sama dilewati dengan alasan tercatat.

UAT **tulis** (mengatur jadwal devYoga dan membuktikan `409` dengan peserta sungguhan) tidak dijalankan: roadmap
menetapkan E2E API-tiruan, dan `409` butuh order terjadwal yang baru tersedia lewat layar booking `FE-MCU-008`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Tampilan sisa kuota per tanggal (mis. *26/30*) | Terpenuhi | §6 Kriteria 1 |
| Kuota di bawah terpakai → pesan `409` server | Terpenuhi | §6 Kriteria 2 |
| Tombol pelampauan hanya bagi `McuSchedule : ApproveQuotaOverride` | Terpenuhi | §6 Kriteria 3 |
| DoD: kriteria terbukti | Terpenuhi | §6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Tindakan pemilik | Jabatan admin MCU perlu `Doctor : Read` (pilihan dokter jaga) dan `CompanyGuarantor : Read` (harga perusahaan paket) di Role Access. Tanpanya select menampilkan pesan `403` server, sedangkan bagian lain layar tetap berfungsi |
| Peringatan | Brief layar memberi contoh *Sen 27-10*; 27-10-2026 sebenarnya Selasa, dan layar mengikuti kalender (*Sel 27-10*). Contoh brief hanya ilustrasi tata letak |
| Masalah yang diketahui | `ApprovedByUserId` pelampauan tidak dikirim dengan nama, sehingga daftar pelampauan sebelumnya hanya menampilkan jumlah dan alasan |
| Dependency backend | Tidak ada; `BE-MCU-007` ✅ |
| Perubahan sampingan | Revisi kecil `FE-MCU-003` dan `FE-MCU-004` (dicatat di laporan masing-masing); satu baris slice `FE-MCU-002` |
| Interupsi | `NONE` |
| Status Git (frontend) | ` M menu-items.jsx` (butir task ini di atas `FE-MCU-003`/`004`); `??` folder route/view/hook/konstanta/utilitas jadwal dan uji baru; berkas MCU lain tetap `??`. Berkas Lab/Radiologi lain yang ` M` bukan bagian task ini |
| Langkah berikutnya | `FE-MCU-006` (lapisan data order dan check-in) — siap; seluruh `MVP-0` frontend kini selesai |
