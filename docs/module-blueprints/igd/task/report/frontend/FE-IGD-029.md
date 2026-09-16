# Laporan Perubahan Frontend — `FE-IGD-029`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-029` |
| Judul | Pendaftaran IGD menutup dengan status Menunggu Triage |
| Slice | `IGD-S01` · `EPIC IGD-01` (pendaftaran dan triage) |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian R3.8 |
| Trace | `IGD-DEC-127`, `IGD-DEC-093`; `FR-IGD-013`, `FR-IGD-015`; evidence [`2026-09-16-kunjungan-terjebak-arrived.md`](../../../evidence/2026-09-16-kunjungan-terjebak-arrived.md) `IGD-EV-131`…`IGD-EV-134` |
| Contract version | State `0.4.0` bagian 1 — `approved` lewat `IGD-DEC-093`. **Nol perubahan kontrak, nol endpoint baru** |
| Wewenang UI | `NOT APPLICABLE` — task ini tidak menyentuh tampilan sama sekali. Nol layar, nol komponen, nol CSS |
| Dependency | `IGD-DEC-127` ✅. Nol dependency backend |
| Klasifikasi | `LIGHT` — satu nilai default berubah pada satu berkas utility, ditambah dua test unit |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (source) + laporan ini pada blueprint IGD |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `2d95904ed` (branch `RizkiV2`) + working tree |
| Commit backend yang dijadikan rujukan | `27351517` (branch `rizkiG`), strict read-only |
| Tanggal | 16 September 2026 |
| Status | **Implementasi selesai; build bersih.** Lint, 859 unit test, dan `npm run build` lulus. Kriteria 3, 4, 5, dan 6 terpenuhi. **Kriteria 1 dan 2 belum terbukti** — keduanya menuntut satu **pendaftaran IGD baru** dijalankan lewat layar; uji layar 16 September 2026 menguji `FE-IGD-030` pada pasien lama, bukan pendaftaran baru |

---

## 1. Keadaan yang ditemukan di awal

Payload pendaftaran IGD mengirim `visitStatus: 1` (`Arrived`) secara eksplisit, bukan membiarkan
backend memilih defaultnya
(`emergency-registration.utils.js` baris 1174-1177, sebelum perubahan).

Pada payload yang sama, dua nilai lain menyatakan hal yang berlawanan:

| Field | Nilai yang dikirim | Artinya |
| --- | --- | --- |
| `registrationStatus` | `Registered` | pendaftaran **sudah tuntas** |
| `registrationCompletedAt` | waktu saat itu | pendaftaran **sudah tuntas** |
| `visitStatus` | `Arrived` | pasien **baru tiba**, belum masuk antrean |

Akibatnya bukan sekadar label yang janggal. Kontrak state bagian 1 hanya mengizinkan `Triaged`
dicapai dari `WaitingForTriage`, sedangkan **tidak ada satu pun** aksi di layar yang memindahkan
kunjungan keluar dari `Arrived` (`IGD-EV-133`, `IGD-EV-134`). Setiap perawat yang menekan
**Simpan Pemeriksaan** pada pasien baru menerima penolakan `409` *"Status kunjungan tidak dapat
berubah dari Arrived ke Triaged."*, dan seluruh pengkajian yang sudah diisi hilang
(`IGD-EV-131`).

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna:** petugas pendaftaran IGD, lalu perawat triage.

Alur normal sesudah perubahan ini:

1. Petugas pendaftaran membuka layar pendaftaran IGD dan mengisi data pasien beserta keluhannya.
2. Petugas menekan **Simpan**. Kunjungan IGD tersimpan dengan status **"Menunggu triage"**.
3. Perawat triage membuka daftar Triage IGD. Pasien tampil dengan badge **"Menunggu triage"**,
   bukan lagi "Pasien tiba".
4. Perawat menekan **Isi Triage**, mengisi pengkajian, lalu menekan **Simpan Pemeriksaan**.
   Penyimpanan **berhasil**, dan status kunjungan berpindah menjadi "Sudah ditriage".

Jalur tidak normal:

| Keadaan | Yang terjadi |
| --- | --- |
| Pemanggil menyertakan `visitStatus` sendiri | Nilai itu tetap dipakai apa adanya. Yang berubah hanya nilai **default** |
| Pasien lama yang terlanjur `Arrived` | **Tidak** ikut berpindah. Jalan keluarnya lewat aksi **Tangani Segera** (`FE-IGD-030`) atau tindakan data terpisah |
| Backend menolak pendaftaran | Perilakunya tidak berubah sama sekali oleh task ini |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `src/utils/health-services/registration-management/emergency-management/emergency-registration.utils.js`
- `src/lib/constants/health-services/registration-management/emergency-management/emergency-registration.constants.js`
- `tests/unit/emergency-registration-payload.test.mjs`, `tests/unit/emergency-visit-status.test.mjs`
- Backend (read-only): `Controllers/EmergencyVisitController.cs`, `Models/EmgVisit.cs`,
  `Services/EmergencyVisitService.cs`, `Enums/EmergencyVisitStatus.cs`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/.../emergency-registration.utils.js` | Default `visitStatus` pada `buildEmergencyVisitPayload` menjadi `EMERGENCY_VISIT_STATUS.WAITING_FOR_TRIAGE`, beserta komentar yang menyebut `IGD-DEC-127` dan alasannya |
| `tests/unit/emergency-registration-payload.test.mjs` | Dua test baru: `FE-IGD-029 K1` (default `WaitingForTriage`) dan `FE-IGD-029 K4` (konteks eksplisit tetap dihormati) |

### 3.3 Kepatuhan arsitektur frontend

Perubahan berada di layer `utils`, tempat pembentukan payload memang tinggal. Nol arsitektur
baru, nol abstraksi baru, nol berkas baru pada `src/`. Konstanta `EMERGENCY_VISIT_STATUS` yang
sudah ada dipakai ulang; tidak ada angka enum yang ditulis tangan — pelajaran yang sama dengan
`FE-IGD-021` dan `FE-IGD-022`, tempat angka enum yang diketik manual mengirim perintah salah
tanpa pernah melempar galat.

---

## 4. State yang ditangani di layar

`NOT APPLICABLE` — task ini tidak mengubah tampilan. Seluruh keadaan memuat, kosong, gagal, dan
tanpa hak akses pada layar pendaftaran maupun daftar triage tetap seperti sebelumnya.

---

## 5. Endpoint yang dikonsumsi

Tidak ada endpoint baru. Endpoint yang payload-nya berubah nilainya:

#### EmergencyVisit

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/emergency-installation-management/emergency-visits` | Menyimpan kunjungan IGD saat pendaftaran; kini mengirim `visitStatus: 2` sebagai default | `EmergencyVisit : Create` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Selesai tanpa keluaran galat | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit` | `859/859` lulus (857 lama + 2 baru) | `PASS` | Keluaran perintah |
| `npm run test:unit` | Tidak dijalankan | `NOT RUN` | Diketahui gagal karena glob skrip, masalah lama yang sama seperti `FE-IGD-023`, `024`, `028` |
| `npm run build` | Berhasil | `PASS` | Dijalankan Rizki 16 September 2026 |
| Daftarkan pasien IGD baru lalu buka daftar triage | **Berhasil.** Pasien yang baru didaftarkan langsung berstatus **"Menunggu Triage"** | `PASS` | Dijalankan pemilik 16 September 2026 |
| Simpan pemeriksaan triage pada pasien baru itu | Belum dijalankan | `NOT RUN` | Pada uji tadi pasien barunya ditekan **Tangani Segera**, bukan **Isi Triage**, sehingga transisi `WaitingForTriage` → `Triaged` belum dilalui lewat layar |

Uji manual: `NOT FEASIBLE` bagi agent — menuntut kredensial petugas pendaftaran dan backend yang
berjalan. **Belum dijalankan pemilik** untuk task ini; yang diuji pada 16 September 2026 adalah
`FE-IGD-030` pada pasien lama.

**Tidak dijalankan:** `npm run test:e2e` dan `test:uat` (tidak diminta task).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Pendaftaran menyimpan `visitStatus = 2`; daftar triage menampilkan "Menunggu triage" | **Terpenuhi** | **Terbukti lewat layar 16 September 2026:** pasien yang baru didaftarkan langsung berstatus "Menunggu Triage". Sisi payload juga terkunci test `FE-IGD-029 K1` |
| 2. Menyimpan triage pada pasien baru berhasil dan status menjadi `Triaged` | **Belum terbukti** | Menuntut satu klik **Isi Triage** pada pasien yang baru didaftarkan, lalu simpan sampai berhasil |
| 3. `registrationStatus` dan `registrationCompletedAt` tidak berubah perilakunya | **Terpenuhi** | Diff hanya menyentuh baris `visitStatus`; test `FE-IGD-001 K2` yang menguji `registrationStatus` tetap lulus |
| 4. Nilai eksplisit dari `context.visitStatus` tetap dihormati | **Terpenuhi** | Test `FE-IGD-029 K4` |
| 5. Pasien lama yang terlanjur `Arrived` tidak ikut berpindah sendiri | **Terpenuhi, dinyatakan apa adanya** | Perubahan hanya pada pembentukan payload kunjungan **baru**; tidak ada migrasi data |
| 6. Nol perubahan backend, `CanTransition`, maupun berkas kontrak | **Terpenuhi** | `git status --short` pada repository backend hanya memuat berkas dokumentasi blueprint |

**Definition of Done gelombang R3.8:** butir lint, unit test, `npm run build`, laporan tracked,
roadmap dan traceability, serta nol komponen bersama/CSS global — **terpenuhi**. Butir catatan
uji layar — **belum**; satu pendaftaran IGD baru masih harus dijalankan lewat layar.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` dari lint dan test |
| Masalah yang diketahui | Dua kunjungan lama pada basis data dev tetap berstatus `Arrived` dan tidak dipindahkan task ini (`IGD-EV-131`) |
| Dependency backend | `NONE` — endpoint dan aturan transisinya sudah ada sejak `BE-IGD-018` |
| Perubahan sampingan | `NONE`. Pekerjaan `FE-IGD-028` yang belum di-commit pada working tree **tidak** disentuh |
| Interupsi | `NONE` |
| Status Git | **Sudah di-commit** pemilik pada `36f122af9` (branch `RizkiV2`), bersama `FE-IGD-028` dan `FE-IGD-030`. Working tree frontend bersih |
| Langkah berikutnya | Daftarkan **satu pasien IGD baru** lewat layar, pastikan badge-nya "Menunggu triage", lalu simpan triage-nya sampai berhasil. Itu menutup kriteria 1 dan 2 sekaligus membuktikan penolakan `409` yang memicu gelombang ini benar-benar hilang |
