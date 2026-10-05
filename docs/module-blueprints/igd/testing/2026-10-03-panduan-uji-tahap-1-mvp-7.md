# Panduan Uji Tahap 1 `MVP-7` — Izin Peran Nyata, Uji Serentak Berhitungan, dan Kueri `IGD-OQ-110`

| Field | Nilai |
| --- | --- |
| Tanggal disusun | 3 Oktober 2026 |
| Untuk | Agen penguji (Antigravity) yang dijalankan pemilik modul (Rizki) |
| Tujuan | Memenuhi syarat audit kesiapan `MVP-7` ([evidence](../evidence/2026-10-03-kesiapan-mvp-7.md)): **C4** izin pada peran nyata, **C5** hitungan baris uji serentak `AT-IGD-174`; dan menjawab **`IGD-OQ-110`** dengan angka |
| Jumlah skenario | 10 uji izin (C4, termasuk tangkapan layar konfigurasi), 3 putaran uji serentak (C5), 2 kueri baca (`IGD-OQ-110`) |
| Kontrak | Permission/audit `0.5.0` §7 (`IGD-DEC-157`); API `0.11.0` §8.3–8.4; validation `0.8.0` §10 |
| Status | Panduan. Belum ada skenario yang dijalankan |

Berkas ini tidak diisi hasil. Hasil ditulis pada laporan baru — lihat bagian 7.

---

## 1. Aturan yang mengikat agen

Seluruh aturan [panduan uji ulang 3 Oktober](2026-10-03-panduan-uji-ulang-fe-igd-036.md) bagian 1 tetap berlaku, ditambah:

| No | Aturan |
| ---: | --- |
| A1 | **Hak akses diberikan pemilik lewat layar Akses Role, bukan oleh agen.** Agen tidak mengubah peran, izin, atau data pengguna dengan cara apa pun. Agen hanya mengambil tangkapan layar halaman Akses Role setiap peran uji sebagai bukti konfigurasi |
| A2 | **Kredensial setiap akun dari variabel lingkungan**: `QUILVIAN_PERAWAT_EMAIL`/`_PASSWORD`, `QUILVIAN_LOKET_EMAIL`/`_PASSWORD`, `QUILVIAN_ADMINDATA_EMAIL`/`_PASSWORD`, `QUILVIAN_TANPAIGD_EMAIL`/`_PASSWORD`, `QUILVIAN_DEV_DB_URL`. **Badan permintaan `POST /auth/login` disamarkan** pada JSON bukti (ganti nilai sandi dengan `"***"`) |
| A3 | **Uji ini tidak memakai SuperAdmin**, kecuali untuk membuat encounter persiapan pada C5 bila akun loket tidak tersedia — catat bila itu terjadi |
| A4 | **Jangan pernah menjalankan rekonsiliasi sungguhan.** Setiap `POST …/emergency-encounter-reconciliations/runs` wajib membawa `expectedCount: 999999` supaya, bahkan bila izinnya keliru, server menolak `409` tanpa menulis apa pun |
| A5 | Kueri SQL hanya `SELECT`. Keluaran setiap kueri disimpan sebagai berkas JSON, bukan hanya diringkas |
| A6 | Layar dilayani hasil build (`npm run start`), dibuktikan `document.querySelector('nextjs-portal') === null` |

---

## 2. Persiapan oleh pemilik (sebelum agen mulai)

Buat atau pakai **empat akun uji** dengan peran berikut lewat layar Akses Role:

| Akun | Izin yang **wajib ada** | Izin yang **wajib tidak ada** |
| --- | --- | --- |
| **Perawat triage** | `EmergencyVisit : Read`, `Create`, `Update`, **`NoShow`**; akses menu Triage Pasien | `EmergencyEncounterReconciliation` (semua aksi) |
| **Petugas loket** | `PatientEncounter : Create`, `Read`; `EmergencyVisit : Create` (pra-cek `active-episode`, `IGD-DEC-158`); akses menu Pendaftaran IGD | `EmergencyVisit : NoShow`; `EmergencyEncounterReconciliation` |
| **Admin data** | `EmergencyEncounterReconciliation : Read`, **`Process`**, **`Reverse`** | Izin klinis IGD — bila peran Anda tetap memberinya, catat apa adanya |
| **Tanpa IGD** | Login dan akses menu Triage Pasien saja | `EmergencyVisit : Read` |

Alamat dasar:

| Resource | Alamat |
| --- | --- |
| Kunjungan IGD | `https://localhost:7184/api/v1/health-services/emergency-installation-management/emergency-visits` |
| Rekonsiliasi | `https://localhost:7184/api/v1/health-services/emergency-installation-management/emergency-encounter-reconciliations` |
| Encounter | `https://localhost:7184/api/v1/health-services/registration-management/patient-encounters` |
| Layar Triage Pasien | `http://localhost:3000/health-services/emergency-installation-management/emergency-triage` |
| Layar Pendaftaran IGD | `http://localhost:3000/health-services/registration-management/emergency-registration` |

---

## 3. Bagian A — Izin pada peran nyata (syarat C4)

| ID | Akun | Langkah | Yang diharapkan | Bukti wajib |
| --- | --- | --- | --- | --- |
| `C4-00` | — | Tangkapan layar halaman Akses Role keempat peran, bagian `EmergencyVisit` dan `EmergencyEncounterReconciliation` | Konfigurasi sesuai tabel bagian 2 | 4 PNG |
| `C4-01` | Petugas loket | Layar Pendaftaran IGD → pasien **bersih** → selesaikan pendaftaran | `GET …/active-episode` `200`; satu `POST /patient-encounters` `200`; nol `POST /emergency-visits`; layar Selesai *Menunggu Triage* | PNG Selesai; JSON jaringan; nomor encounter (dipakai `C4-02`, `C4-04`) |
| `C4-02` | Petugas loket | API: `POST /emergency-visits/no-show` `{ "encounterId": "<C4-01>", "reason": "Uji izin loket" }` | **`403`**; encounter tetap belum berakhir (cek `GET /active-episode?patientId=` → `hasActiveEpisode: true`, `encounter` terisi) | JSON permintaan dan respons |
| `C4-03` | Perawat triage | Layar Triage Pasien → baris `C4-01` → **Aksi → Mulai Triage** → kirim tanpa mengubah isian | `POST /start-triage` `201`; Detail triage terbuka; panel waktu tiba hijau *"Tiba … · dikonfirmasi <nama perawat>"* | PNG; JSON jaringan |
| `C4-04` | Perawat triage | Siapkan encounter baru tanpa kunjungan (ulangi `C4-01` dengan pasien bersih lain), lalu Daftar Triage → **Aksi → Pergi sebelum ditriage** → isi alasan → *Tandai Pergi* | `POST /no-show` `200`; baris hilang; spanduk hijau | PNG; JSON jaringan |
| `C4-05` | Perawat triage | API: `GET /emergency-encounter-reconciliations/preview` lalu `POST /emergency-encounter-reconciliations/runs` `{ "reason": "Uji izin perawat", "expectedCount": 999999 }` | Keduanya **`403`** | JSON |
| `C4-06` | Admin data | API: `GET …/preview` | `200`; catat lima angka kelas (K1, K1-Outpatient, K2, K3, K4) dan `expectedCount`. **Diharapkan K1 + K1-Outpatient = 0** di dev, karena rekonsiliasi sudah dijalankan 23 September dan `BE-IGD-051` menutup encounter sejak itu | JSON |
| `C4-07` | Admin data | API: `POST …/runs` `{ "reason": "Uji izin admin data — tidak boleh menulis", "expectedCount": 999999 }` | **`409`** *"Data berubah sejak pratinjau; muat ulang pratinjau."* — membuktikan izin `Process` lolos tanpa menulis apa pun. **Bila keluar `201`, hentikan uji dan laporkan segera** | JSON |
| `C4-08` | Tanpa IGD | Buka layar Triage Pasien | `GET …/triage-queue` **`403`**; layar menampilkan *"Anda tidak memiliki hak akses untuk melihat data ini."* **tanpa** tombol *Coba lagi* (pengganti simulasi `FE-IGD-035` U10) | PNG; JSON jaringan |
| `C4-09` | — | Kueri baca jejak audit (bagian 3.1) | `C4-03`: `ArrivalConfirmedByUserId` = id akun perawat. `C4-04`: `NoShowByUserId` = id akun perawat, `NoShowReason` = alasan yang diketik | JSON keluaran kueri |

**Catatan pengamatan untuk `C4-02`.** Menu *Pergi sebelum ditriage* ditampilkan menurut status baris dari server,
bukan menurut izin pengguna. Bila agen juga mencobanya lewat layar dengan akun loket, yang diharapkan adalah pesan
`403` tampil apa adanya dan baris tidak berubah. Catat sebagai pengamatan, bukan kegagalan.

### 3.1 Kueri jejak audit (`C4-09`)

```sql
-- Kunjungan hasil C4-03
SELECT v."EmergencyVisitNumber", v."ArrivalTimeSource", v."ArrivalConfirmedByUserId", v."ArrivalConfirmedAt"
FROM public."EmgVisit" v
WHERE v."EncounterId" = '<encounterId C4-01>' AND NOT v."IsDelete";

-- Encounter hasil C4-04
SELECT e."EncounterNumber", e."EncounterStatus", e."NoShowAt", e."NoShowByUserId", e."NoShowReason"
FROM public."RegPatientEncounter" e
WHERE e."Id" = '<encounterId C4-04>';

-- Id akun perawat untuk pembanding
SELECT u."Id", u."Email" FROM public."AspNetUsers" u WHERE u."Email" = '<email perawat dari variabel lingkungan>';
```

Bila nama tabel pengguna berbeda, cari dengan `SELECT table_name FROM information_schema.tables WHERE table_name ILIKE '%user%'`
dan catat tabel yang dipakai.

---

## 4. Bagian B — Uji serentak berhitungan `055-S6` (syarat C5, `AT-IGD-174`)

Jalankan **tiga putaran**, masing-masing dengan pasien bersih yang berbeda.

| Langkah | Tindakan |
| ---: | --- |
| 1 | Buat encounter `Emergency` baru tanpa kunjungan untuk pasien bersih — lewat layar loket dengan akun petugas loket, atau `POST /patient-encounters` (catat caranya). Simpan `encounterId` dan `registeredAt` |
| 2 | Dengan token **perawat triage**, kirim **benar-benar serentak** (`Promise.all`): `POST /start-triage` `{ "encounterId", "mode": "Triage", "arrivalDateTime": "<registeredAt>" }` **dan** `POST /start-triage` `{ "encounterId", "mode": "ImmediateCare" }` |
| 3 | Simpan kedua respons beserta urutan tibanya |
| 4 | Jalankan kueri hitungan di bawah dan simpan keluarannya |

```sql
SELECT COUNT(*) AS jumlah_kunjungan,
       MIN("VisitStatus") AS status_min, MAX("VisitStatus") AS status_max,
       MIN("ArrivalTimeSource") AS sumber
FROM public."EmgVisit"
WHERE "EncounterId" = '<encounterId>' AND NOT "IsDelete";
```

| Yang diharapkan pada setiap putaran | Alasan |
| --- | --- |
| Satu respons `201` dan satu `200`, urutan bebas; kedua respons membawa **id kunjungan yang sama** | Idempoten, `IGD-DEC-143` |
| `jumlah_kunjungan = 1` | Tepat satu kunjungan |
| `status_min = status_max = 4` (`InTreatment`) | Tangani Segera menang, status tidak pernah mundur |
| `sumber` = 2 bila Mulai Triage tiba lebih dulu, 1 bila Tangani Segera lebih dulu | Dicatat saja, bukan syarat lulus |

---

## 5. Bagian C — Kueri `IGD-OQ-110` (dijalankan atas perintah pemilik)

Pertanyaannya: adakah encounter yang **dinonaktifkan** (`IsActive = false`) tetapi **tanpa satu pun dari lima tanda
berakhir**? Bila ada, encounter itu terbaca "masih terbuka" oleh rekonsiliasi, daftar Menunggu Triage, dan penjaga
pendaftaran — pasiennya bisa tertolak saat didaftarkan ulang.

```sql
-- OQ110-A: jumlah per tipe encounter dan status hapus
SELECT e."EncounterType", e."IsDelete",
       COUNT(*) AS jumlah,
       COUNT(*) FILTER (WHERE EXISTS (SELECT 1 FROM public."EmgVisit" v WHERE v."EncounterId" = e."Id")) AS punya_kunjungan_igd
FROM public."RegPatientEncounter" e
WHERE e."IsActive" = false
  AND NOT e."IsCancel"
  AND e."CancelledAt" IS NULL
  AND e."CompletedAt" IS NULL
  AND e."NoShowAt" IS NULL
  AND e."EncounterStatus" NOT IN (9, 10, 11)
GROUP BY e."EncounterType", e."IsDelete"
ORDER BY e."EncounterType", e."IsDelete";

-- OQ110-B: contoh 20 baris Emergency (2) untuk melacak jalur penonaktifannya
SELECT e."EncounterNumber", e."EncounterStatus", e."RegisteredAt", e."UpdateDateTime", e."UpdateBy", e."IsDelete"
FROM public."RegPatientEncounter" e
WHERE e."EncounterType" = 2
  AND e."IsActive" = false
  AND NOT e."IsCancel"
  AND e."CancelledAt" IS NULL AND e."CompletedAt" IS NULL AND e."NoShowAt" IS NULL
  AND e."EncounterStatus" NOT IN (9, 10, 11)
ORDER BY e."UpdateDateTime" DESC
LIMIT 20;
```

Simpan keduanya apa adanya. Bila nama kolom `RegisteredAt`, `UpdateDateTime`, atau `UpdateBy` berbeda, sesuaikan dan
catat penyesuaiannya. **Yang diharapkan:** tidak ada — hasil ini menjawab pertanyaan, bukan lulus/gagal.

---

## 6. Bentuk bukti mentah

Folder baru: `QuilvianSystemFrontendDev/test-with-agy/igd/uji-tahap-1-2026-10-03/`

| Berkas | Isi |
| --- | --- |
| `<ID>.json` | `{ id, akun (peran, bukan email), percobaan, mulai, selesai, nextjsPortalNull (uji layar), langkah[], jaringan[], kueri[], pemeriksaan{}, putusan, alasan }` — sandi login disamarkan |
| `<ID>.png` | Tangkapan layar viewport sesudah respons + 1000 ms |
| `C5-putaran-<n>.json` | Kedua respons serentak dengan stempel waktu, keluaran kueri hitungan |
| `OQ110-A.json`, `OQ110-B.json` | Keluaran kueri apa adanya |
| `persiapan.json` | SHA frontend dan backend, waktu `BUILD_ID`, daftar akun (peran saja), hasil `C4-00` |

---

## 7. Pelaporan

Laporan baru di folder ini: `2026-10-03-laporan-uji-tahap-1-mvp-7.md`. Panduan ini, laporan task, roadmap, dan dokumen
blueprint lain **tidak disunting**.

| Bagian | Isi |
| --- | --- |
| Metadata | Tanggal, pelaksana, SHA, bukti hasil build, basis data dev **tanpa** alamat host |
| C4 | Tabel per skenario: akun, `PASS` / `FAIL` / `NOT RUN`, kode status, kalimat, berkas bukti |
| C5 | Tabel per putaran: urutan respons, kode status, id kunjungan, `jumlah_kunjungan`, status akhir, sumber |
| `IGD-OQ-110` | Angka OQ110-A per tipe, dan ringkasan OQ110-B |
| Penyimpangan | Setiap percobaan ulang, data pengganti, dan pemakaian SuperAdmin (aturan A3) |

| Bagian | Jumlah | `PASS` | `FAIL` | `NOT RUN` |
| --- | ---: | ---: | ---: | ---: |
| C4 | 10 (`C4-00`…`C4-09`) | | | |
| C5 | 3 putaran | | | |
| `IGD-OQ-110` | 2 kueri | — | — | |
