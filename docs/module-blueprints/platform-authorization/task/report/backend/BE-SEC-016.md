# Laporan Perubahan Backend — `BE-SEC-016`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-SEC-016` |
| Judul | Penutupan matriks pemilik HR — posisi `Staff HR` dan penegakan kardinalitas 24 + 12 = 36 |
| Slice | Task terpisah di luar rantai — menutup gerbang terbuka `BE-SEC-014` |
| Roadmap | [`../../../roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) — bagian *Task terpisah di luar rantai* |
| Trace | [`evidence/14`](../../../evidence/14-owner-policy-matrix-and-deployment-preparation.md) bagian H, [`BE-SEC-014.md`](BE-SEC-014.md) |
| Contract version | `NOT APPLICABLE` — tidak ada kontrak API yang disentuh |
| Dependency | `BE-SEC-014` (`f209a376`), `BE-SEC-015` (`5ab741ca`) — keduanya selesai dan sudah di-commit |
| Klasifikasi | `MEDIUM` — nol perubahan source aplikasi; satu skrip database dan tiga dokumen |
| Task mode | `BACKEND` |
| Target tulis | `Migrations/scripts/`, `docs/module-blueprints/platform-authorization/` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `5ab741ca88f3d5bd13ce1476fc40e9eb4da134fa` |
| Tanggal | 17 September 2026 |
| Status | **Selesai.** Skrip siap; tidak dijalankan |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` (HR) dan Shared Platform (otorisasi) |
| Module | `HUMAN_RESOURCE_MASTER_DATA`, `HUMAN_RESOURCE_SCHEDULING` |
| Submodule | `AttendanceAndSchedule`, `SchedulingManagement` |
| Pemilik/prefix pada registry | `Hrd`, `Wfp` — tidak ada perubahan registry |
| Keberlakuan | `TOUCHED LEGACY` — **nol berkas source aplikasi berubah pada task ini** |
| Status registry | Tidak berubah. Verifier tetap `1.300 / 340 / 48`, `BE-SEC-003` 24/24 |
| QBE ID yang berlaku | Tidak ada. Tidak ada entity, rename, migration, maupun eksekusi database |
| Wewenang database | **TIDAK DIBERIKAN dan TIDAK DIPAKAI.** Skrip pemberian hak tidak dijalankan |
| Pemilihan ID task | `BE-SEC-004`–`BE-SEC-011` dipesan sebagai dekomposisi `BE-SEC-002` (`NOT STARTED`); `BE-SEC-012`–`BE-SEC-015` terpakai. `BE-SEC-016` diverifikasi bebas pada empat sumber: roadmap, laporan tracked, working tree, dan riwayat Git (`git log --all --grep` dan `--diff-filter=A`) |

---

## 1. Masalah yang diperbaiki

`BE-SEC-014` menyiapkan skrip pemberian hak awal HR tetapi sengaja membiarkan satu gerbang terbuka:
**daftar posisi staff HR belum disetujui pemilik.** Skrip menolak menebak, sehingga bagian 3.2-nya
dibiarkan kosong dan hanya `Manajer HR` yang dilayani.

Keadaan itu aman tetapi belum selesai: seluruh staf HR tidak memperoleh akses apa pun ke enam
resource master data kepegawaian, termasuk `Read`.

Pemilik sistem kini menetapkan keputusannya secara final, berdasarkan audit baca-saja terhadap
database yang mengonfirmasi Departemen `Human Resource` hanya memiliki dua posisi.

---

## 2. Proses bisnis — matriks pemilik yang final

| Departemen × Posisi | Read | Create | Update | Delete |
|---|:---:|:---:|:---:|:---:|
| `Human Resource` × `Manajer HR` | ✅ | ✅ | ✅ | ✅ |
| `Human Resource` × `Staff HR` | ✅ | ✅ | ⛔ | ⛔ |

Enam resource: `WorkSchedule`, `Shift`, `ShiftGroup`, `ShiftPattern`, `WorkCalendar`,
`WorkScheduleAssignment`.

### 2.1 Lingkup Finance — sempit, dan sengaja

`BE-SEC-016` **tidak** memutuskan apa pun tentang Finance. Satu-satunya pembatasan Finance yang
disetujui pemilik sudah ditetapkan lebih dulu dan tidak diperluas di sini:

| Pembatasan Finance yang disetujui | Ditangani oleh |
|---|---|
| `Finance` × `Manajer Finance` — `WorkSchedule.Update` tidak dipertahankan | `be-sec-014-pre-seeder-revoke-finance-workschedule.sql` |
| `Finance` × `Manajer Finance` — `WorkSchedule.Delete` tidak dipertahankan | skrip yang sama |

**Tetap berlaku apa adanya, di luar scope task ini:** `WorkSchedule.Read` dan
`WorkSchedule.Create` milik Finance; seluruh izin Finance pada `Shift`, `ShiftGroup`,
`ShiftPattern`, `WorkCalendar`, dan `WorkScheduleAssignment`; serta posisi Finance selain
`Manajer Finance`.

Skrip pemberian hak HR tidak memberi, mencabut, memeriksa, maupun melarang satu pun dari itu.
Pernyataan "Finance tidak memperoleh apa pun" **tidak** boleh disimpulkan dari task ini.

#### Penyempitan penegasan yang diwariskan `BE-SEC-014`

Penegasan Finance pada skrip pemberian hak sebelumnya lebih luas daripada keputusan pemilik, dan
`BE-SEC-016` mempersempitnya. Perbedaannya nyata, bukan kosmetik:

| Aspek | Bentuk lama (`BE-SEC-014`) | Bentuk sekarang (`BE-SEC-016`) |
| --- | --- | --- |
| Departemen | `ILIKE '%finance%'` — menjaring departemen mana pun yang memuat kata itu | `= 'Finance'` — cocok persis |
| Posisi | tidak disaring — seluruh posisi Finance | `= 'Manajer Finance'` |
| Resource | keenam resource HR | `= 'WorkSchedule'` saja |
| Action | `Update`, `Delete` | `Update`, `Delete` (tidak berubah) |

Bentuk lama akan membatalkan pemberian hak HR karena izin Finance yang **tidak pernah** menjadi
keputusan pemilik — misalnya `Shift.Update` milik posisi Finance mana pun, atau departemen bernama
`Finance & Accounting`. Bentuk sekarang hanya memastikan pencabutan pra-seeder yang disetujui memang
sudah dijalankan.

Perubahan ini berlaku pada penegasan 3.9(e) dan pada query verifikasi 4.2. Dokumentasi historis
`BE-SEC-014` yang menggambarkan bentuk lama **tidak ditulis ulang** — ia benar pada saat itu, dan
penyempitannya dicatat di sini.

| Pasangan | Perhitungan | Kunci alami |
|---|---|---:|
| `Manajer HR` | 6 × 4 | **24** |
| `Staff HR` | 6 × 2 | **12** |
| **Total cakupan akhir** | | **36** |

**36 adalah cakupan akhir, bukan jumlah `INSERT`.** Skrip tetap idempoten: kunci alami yang sudah
ada dan masih efektif dipertahankan, hanya yang belum ada yang disisipkan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Kunci bisnis — nama atau code?

Pertanyaan ini ditinjau ulang secara eksplisit sebelum menulis satu baris pun.

| Kunci | Jaminan database | Sumber |
| --- | --- | --- |
| `MstDepartment."DepartmentCode"` | **UNIQUE**, tanpa filter | `MstDepartmentConfiguration.cs:51` |
| `MstPosition ("DepartmentId","PositionCode")` | **UNIQUE**, tanpa filter | `MstPositionConfiguration.cs:59` |
| `MstDepartment."DepartmentName"` | indeks biasa — **TIDAK unik** | `MstDepartmentConfiguration.cs:54` |
| `MstPosition ("DepartmentId","PositionName")` | indeks biasa — **TIDAK unik** | `MstPositionConfiguration.cs:62` |

Jadi **code memang kunci kanonik** pada repository ini, dan nama tidak dijamin unik.

**Keputusan: pencarian berbasis nama DIPERTAHANKAN, dengan silang-periksa code.** Alasannya:

1. Keputusan pemilik dan audit baca-saja yang mendasarinya sama-sama dinyatakan dalam **nama**
   (`Human Resource`, `Manajer HR`, `Staff HR`). Mengganti selektor menjadi code berarti memakai
   kunci yang tidak pernah ditinjau maupun disetujui pemilik.
2. Ketidakunikan nama ditutup dengan cara lain: setiap pencarian menegaskan **tepat satu** baris,
   sehingga nama ambigu **membatalkan transaksi** alih-alih diam-diam memilih satu baris.
3. Code tidak diabaikan — `DepartmentCode` dan `PositionCode` dibaca ulang dan dicetak ke `NOTICE`
   supaya operator dapat mencocokkannya dengan keluaran bagian 2.1/2.3 skrip pembacaan sebelum
   `COMMIT`.

Tidak ada konvensi kunci baru yang diperkenalkan.

### 3.2 Gerbang sebelum menulis (3.4 dan 3.7)

| Penegasan | Perilaku bila gagal |
| --- | --- |
| Departemen `Human Resource` tepat 1 baris aktif | Batalkan transaksi |
| Posisi `Manajer HR` tepat 1 baris pada departemen itu | Batalkan transaksi |
| **Setiap** posisi staff tepat 1 baris — bukan sekadar "ada" | Batalkan transaksi, sebut nama dan jumlah yang ditemukan |
| Sasaran manajer = **24** | Batalkan transaksi |
| Sasaran staff = **12** | Batalkan transaksi |
| Sasaran `Update`/`Delete` untuk staff = **0** | Batalkan transaksi, sebut pasangan terlarangnya |
| Total sasaran = **36** | Batalkan transaksi |
| Sasaran untuk posisi di luar matriks = **0** | Batalkan transaksi |

Larangan `Update`/`Delete` untuk staff dihitung **langsung** sebagai pasangan terlarang, bukan
disimpulkan dari total — sehingga tetap terdeteksi walaupun jumlahnya kebetulan pas.

### 3.3 Penegasan sesudah menulis (3.9), masih di dalam transaksi

| Penegasan | Nilai wajib |
| --- | --- |
| Seluruh sasaran efektif (`IsAllowed` + `IsActive` + `NOT IsDelete`) | 36 / 36 |
| Cakupan `Manajer HR` | **24 / 24** |
| Cakupan `Staff HR` | **12 / 12** |
| `Staff HR` memegang `Update`/`Delete` efektif pada enam resource | **0** |
| `Finance` × `Manajer Finance` memegang `WorkSchedule.Update`/`Delete` efektif — **hanya itu yang diperiksa** | **0** |
| Kunci alami ganda pada `SysAccessPolicy` | **0** |
| Baris yang disisipkan di luar matriks 36 kunci | **0** |

Dua hal yang patut dicatat:

- Larangan `Staff HR` `Update`/`Delete` dibaca **dari database**, bukan dari matriks sasaran. Kalau
  hak itu ada dari sumber lain sekalipun, transaksi tetap dibatalkan.
- Baris yang benar-benar disisipkan direkam lewat `RETURNING` ke tabel sementara, sehingga
  penegasan "tidak ada izin di luar 36 kunci" memeriksa **daftar nyata**, bukan perkiraan.

### 3.4 Gagal tertutup untuk kunci alami yang tidak aman dipakai ulang

Bagian 3.8 melewati kunci alami yang sudah ada — termasuk yang **nonaktif** atau **soft-deleted**.
Baris seperti itu kemudian muncul pada 3.9(a) sebagai sasaran yang belum efektif dan **membatalkan
transaksi**, dengan pesan yang menyebut pasangan mana yang bermasalah.

Skrip ini **tidak pernah** menghidupkan ulang policy yang pernah dicabut. Keputusan itu milik
pemilik sistem, bukan skrip.

### 3.5 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NONE` |
| Source aplikasi | `NONE` — nol berkas `.cs` berubah |
| Skema database | `NONE` — tidak ada EF migration |
| Eksekusi database | `NONE` — skrip tidak dijalankan, tetap berakhir `ROLLBACK` |
| Keamanan | **Positif.** Staf HR memperoleh akses baca/buat yang memang dibutuhkan, tanpa satu pun jalur `Update`/`Delete`, dan penegakannya berlapis sebelum maupun sesudah menulis |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE`. Tidak ada endpoint yang dibuat, diubah, atau dihapus.

---

## 5. Verifikasi

### 5.1 Build dan verifier

| Perintah | Hasil |
| --- | --- |
| `dotnet build -c Release --no-incremental` | **0 Error(s)**, 189 warning dokumentasi XML yang sudah ada sebelumnya |
| `tools/authorization-verifier/verify-authorization.sh` | **PASS** |
| `git diff --check` | Bersih, exit 0 |

| Metrik | Diharapkan | Terukur |
| --- | --- | --- |
| Actions | 1.300 | 1.300 ✅ |
| Resources | 340 | 340 ✅ |
| Modules | 48 | 48 ✅ |
| Metadata gaps | 0 | 0 ✅ |
| Fallback | 69 persis | 69 cocok persis ✅ |
| `BE-SEC-003` | 24/24 | 24/24 ✅ |
| Naked baru | 0 | 0 ✅ |
| Naked baseline | 0 | 0 ✅ |
| Identitas kanonik ganda | 0 | tidak ganda ✅ |

Nol perubahan source membuat hasil verifier identik dengan `5ab741ca` — itu memang buktinya: task
ini tidak menyentuh perilaku otorisasi aplikasi.

### 5.2 Keamanan SQL statik

| Pemeriksaan | Hasil |
| --- | --- |
| Kompatibel DBeaver | ✅ `set_config`/`current_setting`, SQL biasa |
| Meta-command psql | ✅ **0** — nol *backslash* pada berkas |
| Interpolasi variabel psql | ✅ **0** |
| Validasi UUID operator | ✅ Tidak kosong, menolak nol-GUID, wajib ada di `AspNetUsers` |
| GUID entity yang di-*hardcode* | ✅ **Nihil** — satu-satunya GUID literal adalah nol-GUID (placeholder operator dan `Guid.Empty`) |
| Kolom `NOT NULL` `SysAccessPolicy` | ✅ 14 kolom, 14 nilai |
| Perlindungan kunci alami | ✅ `NOT EXISTS` atas empat kolom |
| Kardinalitas 24 + 12 = 36 | ✅ Ditegakkan sebelum dan sesudah menulis |
| Akhiran default | ✅ `ROLLBACK` |
| Perluasan izin Finance | ✅ Nihil — tidak ada izin Finance yang diberikan |
| Pencabutan izin Finance di luar keputusan pemilik | ✅ Nihil — skrip tidak mencabut apa pun; penegasannya pun hanya membaca `Manajer Finance` × `WorkSchedule.Update`/`Delete` |
| `Staff HR` `Update`/`Delete` | ✅ Nihil; ditegaskan dua kali |
| Pemberian di luar matriks | ✅ Nihil; ditegaskan dari daftar `RETURNING` |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. ID task berikutnya diverifikasi, bukan diasumsikan | Terpenuhi | Preflight — empat sumber diperiksa |
| 2. Posisi staff yang disetujui dikodekan persis | Terpenuhi | Bagian 3.2 skrip — `Staff HR`, satu baris |
| 3. Tidak ada posisi HR lain yang ditebak atau ditambahkan | Terpenuhi | Gerbang 3.7(d) menolak posisi di luar matriks |
| 4. Matriks final 24 + 12 = 36 | Terpenuhi | Bagian 3.2 laporan ini |
| 5. Idempoten; hanya kunci alami yang hilang disisipkan | Terpenuhi | `NOT EXISTS` atas kunci alami |
| 6. Gagal tertutup untuk kunci alami yang ada tetapi tidak aman dipakai ulang | Terpenuhi | Bagian 3.4 |
| 7. Tepat satu departemen dan tepat satu per posisi | Terpenuhi | Gerbang 3.4(a)(b)(c) |
| 8. Kunci bisnis stabil ditinjau ulang | Terpenuhi | Bagian 3.1 — code kanonik, nama dipertahankan + silang-periksa |
| 9. Tidak ada GUID yang di-*hardcode* | Terpenuhi | Bagian 5.2 |
| 10. Bukti eksplisit nol sasaran `Update`/`Delete` untuk staff | Terpenuhi | Gerbang 3.7(c) dan penegasan 3.9(d) |
| 11. Verifikasi sesudah tulis di dalam transaksi | Terpenuhi | Bagian 3.3 |
| 12. Penegasan Finance dibatasi pada keputusan pemilik saja | Terpenuhi | Penegasan 3.9(e) — `Finance` × `Manajer Finance` × `WorkSchedule` × `Update`/`Delete`, nama departemen dicocokkan persis; izin Finance lain tidak diperiksa |
| 13. Tidak ada izin di luar 36 kunci | Terpenuhi | Penegasan 3.9(f), dari daftar `RETURNING` |
| 14. Default `ROLLBACK` dipertahankan | Terpenuhi | Bagian 5.2 |
| 15. Evidence historis tidak ditulis ulang | Terpenuhi | `evidence/14` bagian A–G utuh; penyelesaian ditambahkan sebagai bagian H |
| 16. Build `Release` dan verifier dijalankan ulang | Terpenuhi | Bagian 5.1 |
| 17. Tidak ada eksekusi database, seeder, atau aplikasi | Terpenuhi | Bagian 5 |

**Butir Definition of Done yang belum terpenuhi — disebut apa adanya:**

| Butir | Keadaan |
| --- | --- |
| Skrip diuji terhadap database | **Belum.** Rancangan yang belum pernah dijalankan |
| Pemegang `Read`/`Create` saat ini | **Belum terukur ulang** sejak audit pemilik |
| Penerapan | **Belum.** Urutan sepuluh langkah `evidence/14` bagian E belum dijalankan |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build memunculkan 189 warning dokumentasi XML yang sudah ada sebelumnya |
| Masalah yang diketahui | Skrip belum pernah dijalankan terhadap database mana pun |
| Risiko tersisa | **1.** Ejaan `Human Resource`, `Manajer HR`, dan `Staff HR` diambil dari audit pemilik; bila berubah di database, gerbang 3.4 membatalkan transaksi — perilaku yang diinginkan, tetapi operator sebaiknya menjalankan ulang bagian 2.1 skrip pembacaan lebih dulu. **2.** Bila sebuah kunci alami sasaran sudah ada tetapi nonaktif, transaksi dibatalkan dan pemilik harus memutuskan apakah baris itu boleh dihidupkan — skrip sengaja tidak memutuskannya sendiri |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Langkah berikutnya | **1.** Operator menjalankan ulang bagian 2.1 skrip pembacaan untuk mengonfirmasi ejaan. **2.** Jalankan dry-run bagian 3.6 — total sasaran wajib 36. **3.** Jalankan urutan sepuluh langkah `evidence/14` bagian E. **4.** Commit/push menunggu instruksi terpisah |
