# Human Resource — Paket Tinjauan Keamanan

| Field | Value |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Dokumen | `evidence/02-security-review-packet.md` |
| Jenis | **Paket tinjauan**, bukan kontrak. Bukan bagian dari ketiga belas artefak canonical |
| Status | **`SECURITY_APPROVED`** — dijawab 2026-08-30 |
| Disiapkan oleh | Pemilik teknis dan produk (`HRD-DEC-015`), 2026-08-30 |
| Untuk | Pemilik keamanan |
| Keputusan yang ditinjau | `HRD-DEC-032`, `HRD-DEC-033` |
| Status kedua keputusan | **`SECURITY_APPROVED`** |
| Otoritas keamanan | `Project final decision authority — Security`, dinyatakan eksplisit dalam percakapan. Governance tidak menyediakan field nama orang, dan dokumen ini **tidak mengarang nama siapa pun** |
| Backend SHA | `e0ee42c752a5f92c5b1663ff88bef07a5859f79f` |

---

## 0. Untuk apa dokumen ini ada

Dua keputusan pemilik produk menyentuh keamanan dan privasi, sehingga keduanya memerlukan
tanda tangan kedua dari pemilik keamanan. Dokumen ini dibuat supaya **keamanan tidak perlu
membaca seluruh blueprint HR** — seluruh yang dibutuhkan untuk memutuskan ada di sini.

**Yang diminta dari keamanan:** satu dari dua jawaban per bagian —

| Jawaban | Artinya |
| --- | --- |
| `APPROVE` | Model diterima. Keputusan naik dari `OWNER_DECIDED_PENDING_SECURITY_COSIGN` menjadi `SECURITY_APPROVED`. **Inilah yang terjadi** |
| `REQUEST_CHANGES` | Model perlu diubah. Sebutkan butir mana dan mengapa |

**Yang tidak diminta:** membaca arsitektur, kontrak API, kamus data, maupun flowchart. Ketiganya
dirujuk bila diperlukan, tetapi bukan syarat untuk menjawab.

**Keadaan dokumen ini sekarang:** tinjauan **sudah selesai**. Kedua bagian dijawab `APPROVE` oleh
`Project final decision authority — Security` pada 2026-08-30, beserta empat keputusan sasaran
tambahan yang dicatat pada decision log bagian 28. Isi paket di bawah dipertahankan apa adanya
sebagai bahan yang ditinjau, bukan diubah setelah keputusan diberikan.

---

## Bagian A — `HRD-DEC-032`: Model Peran ke Hak Akses

### A.1 Keputusan pemilik yang ditinjau

Usulan peta peran pada `contracts/permission-audit-matrix.md` ditetapkan sebagai
**`FUNCTIONAL ROLE BASELINE`**, dengan satu larangan yang menyertainya: **nama peran fungsional
MUST NOT dianggap sama dengan peran aplikasi pada Identity.**

### A.2 Bukti keadaan sekarang — hasil audit read-only

| Yang diperiksa | Temuan | Bukti |
| --- | --- | --- |
| Model peran | `ApplicationRole : IdentityRole<Guid>`, punya penanda `IsSystemRole` | `Models/ApplicationRole.cs` |
| Peran yang benar-benar di-seed | **Hanya dua: `SuperAdmin` dan `User`** | `Seeders/SuperAdminSeeder.cs` baris 9–10 |
| Peran HR | **Tidak ada satu pun.** Peran lain dibuat administrator saat aplikasi berjalan | — |
| Katalog butir hak akses | **Dibangkitkan mesin** dari atribut `[AccessController]` dan `[AccessAction]` | `Seeders/AccessMenuSeeder.cs` |
| Pengikat peran ke aksi | `SysAccessPolicy`, dibuat saat berjalan | `RoleAccessController` — `POST /policies`, `POST /policies/copy` |
| Urutan penegakan | Pengguna → peran Identity → kebijakan akses → aksi | `Filters/AccessPermissionFilter.cs`, `Services/Security/AccessPermissionService.cs` |
| Cakupan `[Authorize]` | **152 dari 152** controller HR. **Nol** `[AllowAnonymous]` | Diverifikasi ulang 2026-08-30 |

**Ringkasnya:** sisi hak akses **sudah lengkap dan dibangkitkan mesin**; sisi peran **belum ada**.

### A.3 Model peran ke hak akses yang diusulkan

| Peran fungsional | Peran Identity yang ada | Butir hak akses | Cakupan data | Penjaga tingkat baris atau domain | Aksi sensitif | Status pemetaan |
| --- | --- | --- | --- | --- | --- | --- |
| **Employee** | **tidak ditemukan** | Butir `My*`: `Read`, `Create`, `Update`, `Submit`, `Cancel`, `Delete` | **Hanya data dirinya sendiri** | Kepemilikan diturunkan dari pengguna yang masuk, bukan dari parameter permintaan | — | `MAPPING_REQUIRED` |
| **Supervisor / Manager** | **tidak ditemukan** | Butir `My*` miliknya; `ApprovalInbox : Read`, `: Approve`, `: Reject`, `: RequestRevision`, `: Return`; `AttendanceException : Classify` | Anak buah menurut penetapan atasan yang berlaku | Tugas persetujuan hanya yang **ditugaskan kepadanya** | Menyetujui pengajuan yang berdampak pada upah | `MAPPING_REQUIRED` |
| **HR Admin** | **tidak ditemukan** | Master data HR; butir `Wfp*`; `AttendanceDaily : Read`; `AttendanceRawLog : Read`, `: Create`, `: Update`; `AttendanceCorrection : Read`, `: Apply`, `: CreateOnBehalf`; `LeaveBalance : Read`; `LeaveAdjustment` lengkap; `OvertimePlan` lengkap; penjadwalan | Seluruh pegawai dalam cakupan lokasi | Pengajuan atas nama **wajib** menyimpan pemrakarsa, pegawai yang diwakili, dan alasannya | `AttendanceCorrection : CreateOnBehalf` — bertindak atas nama orang lain | `MAPPING_REQUIRED` |
| **HR Manager** | **tidak ditemukan** | Seluruh butir HR Admin; ditambah `Wfp{Salary,Organization,Position,Manager}Assignment : Approve`; ditambah `WfpSalaryAssignment : ViewAmount` | Seluruh pegawai dalam cakupan lokasi | **MUST** berbeda dari pemrakarsa pada setiap persetujuan `T8` | Menyetujui perubahan gaji dan penempatan; membaca nominal gaji | `MAPPING_REQUIRED` |
| **Payroll Officer** | **tidak ditemukan** | `AttendancePeriod : Create`, `: Close`, `: Reopen`, `: Cancel`; `AttendancePayrollHandoff : Execute`, `: Rollback`; kesiapan payroll sisi HR; `WfpSalaryAssignment : ViewAmount` | Seluruh pegawai dalam cakupan periode | Penutupan periode ditahan bila masih ada pengecualian pemblokir | Menutup dan membuka kembali periode; membaca nominal gaji | `MAPPING_REQUIRED` |
| **Scheduling Lead** | **tidak ditemukan** | `RosterPeriod`, `ShiftAssignment`, `ScheduleChangeRequest`, `ShiftSwapRequest` — seluruh aksi | Unit yang menjadi tanggung jawabnya | Bentrok lisensi dan kewenangan klinis **MUST NOT** dapat dilewati penetapan manual | Menerbitkan jadwal yang mengikat banyak orang | `MAPPING_REQUIRED` |
| **Auditor** | **tidak ditemukan** | Hanya aksi `Read` lintas domain HR | Seluruh pegawai, baca saja | Tidak ada aksi yang mengubah data | **Tidak** memegang `: ViewAmount` maupun butir kasus kedisiplinan | `MAPPING_REQUIRED` |
| — | **`SuperAdmin`** | **Melewati seluruh pemeriksaan** kecuali aksi bertanda khusus sistem | Tidak dibatasi | Tidak ada | Seluruhnya | `EXISTS` — bukan peran HR |
| — | **`User`** | Peran bawaan, tanpa kebijakan akses HR | — | — | — | `EXISTS` — tidak mencukupi |

### A.4 Tujuh hal yang diminta dinilai keamanan

| # | Pertanyaan | Konteks yang perlu diketahui |
| ---: | --- | --- |
| 1 | Apakah **baseline peran fungsional** di atas dapat diterima sebagai titik awal? | Ia usulan, bukan konfigurasi terpasang. Menyetujuinya berarti menyetujui bentuknya, bukan memasangnya |
| 2 | Apakah **pemisahan peran** sudah cukup? | `HRD-DEC-031` menuntut penyetuju berbeda dari pemrakarsa pada `T8`. Hari ini **belum dijaga** — endpoint persetujuan gaji memakai butir `: Update` yang sama dengan buat dan ubah. Butir `: Approve` terpisah diusulkan justru untuk menutup celah ini |
| 3 | Apakah **`SuperAdmin` yang melewati seluruh pemeriksaan** dapat diterima sebagai perilaku darurat dan administrasi? | Ini perilaku yang **sudah ada**, bukan usulan baru. Konsekuensinya: pemegang `SuperAdmin` dapat membaca nominal gaji dan menyetujui perubahan penempatan tanpa melewati pemisahan peran |
| 4 | Apakah **butir sensitif HR Manager terlalu luas**? | HR Manager memegang persetujuan `T8` **dan** `: ViewAmount` sekaligus. Pertanyaannya: apakah keduanya boleh berada pada satu peran |
| 5 | Apakah **cakupan Payroll Officer** sesuai? | Ia memegang penutupan dan pembukaan kembali periode, serta `: ViewAmount`. Siapa yang berwenang membuka kembali periode masih `[OPEN]` — `HRD-Q-23` |
| 6 | Apakah **Auditor benar-benar hanya baca**? | Usulan: hanya aksi `Read`, tanpa `: ViewAmount`, tanpa akses kasus kedisiplinan. Apakah pembatasan itu tepat |
| 7 | Apakah **`ViewAmount` harus terpisah dari `Read`/`ReadAll`**? | Ini inti Bagian B. Bila digabung, setiap pemegang butir baca umum dapat membaca nominal gaji seluruh pegawai |

### A.5 Yang MUST NOT terjadi sebagai hasil tinjauan ini

| Larangan | Sebabnya |
| --- | --- |
| Membuat peran baru pada source aplikasi | Pembuatan peran adalah tindakan administrator pada aplikasi berjalan, bukan perubahan kode |
| Mengarang nama peran Identity | Peta yang menunjuk peran tidak ada tidak menjaga apa pun |
| Memakai `SuperAdmin` sebagai pengganti peran HR | Ia melewati seluruh pemeriksaan; memakainya meniadakan matriks kewenangan |
| Menutup `MAPPING_REQUIRED` tanpa peran Identity yang benar-benar dibuat | Statusnya baru sah tertutup setelah peran ada dan terikat kebijakan akses |

### A.6 Keputusan keamanan — diisi pemilik keamanan

| Field | Isi |
| --- | --- |
| Keputusan | **`APPROVE`** |
| Ketentuan yang menyertai | 1. Peran fungsional tetap menjadi model otorisasi HR. 2. Pemetaan ke peran Identity tetap lewat konfigurasi runtime administrator. 3. `MAPPING_REQUIRED` **bukan** alasan mengarang peran di source. 4. `SuperAdmin` **MUST NOT** dipakai sebagai pengganti peran HR pada operasi normal; ia tetap otoritas administratif dan darurat sesuai perilaku platform yang ada |
| Otoritas peninjau | `Project final decision authority — Security` |
| Tanggal | 2026-08-30 |

**Jawaban atas tujuh pertanyaan bagian A.4:** seluruhnya diterima dengan ketentuan di atas.
Butir 3 dijawab tegas — `SuperAdmin` yang melewati seluruh pemeriksaan **diterima** sebagai
perilaku administratif dan darurat, **bukan** sebagai peran operasional HR. Butir 7 dijawab
tegas — `ViewAmount` **wajib** terpisah dari `Read` dan `ReadAll`.

---

## Bagian B — `HRD-DEC-033`: Keterlihatan Nominal Gaji

### B.1 Keputusan pemilik yang ditinjau

**`SALARY_AMOUNT_HIDDEN_BY_DEFAULT`.**

### B.2 Model yang diusulkan

#### Daftar lintas pegawai

Nominal gaji **`NOT_RETURNED`** — **tidak dikembalikan backend**, bukan sekadar disembunyikan
frontend.

| Yang boleh tampil | Yang tidak |
| --- | --- |
| Pegawai | **Nominal gaji dalam bentuk apa pun** |
| Organisasi atau unit | Nilai tunjangan |
| Jabatan atau kelas gaji | Nilai potongan |
| Mata uang | — |
| Tanggal berlaku | — |
| Penanda utama | — |
| Status persetujuan | — |

**Alasan bentuknya begitu.** Nilai yang tetap dikirim lalu disembunyikan di layar **tetap
melintas jaringan** dan tetap terbaca siapa pun yang membuka alat pengembang peramban.
Penyembunyian di layar bukan kendali keamanan.

#### Detail gaji satu pegawai

Nominal dikembalikan **hanya** bila pengguna memegang `WfpSalaryAssignment : ViewAmount`, atau
butir canonical yang setara pada kontrak.

#### Laporan, ekspor, dan keterlihatan massal

**Tidak tersedia pada MVP administratif.**

Keterlihatan massal di masa depan memerlukan butir terpisah `WfpSalaryAssignment : ViewAmountBulk`
atau setaranya, **dan** persetujuan keamanan yang baru — bukan turunan dari persetujuan ini.

### B.3 Aturan yang paling menentukan

> **`Read` dan `ReadAll` TIDAK menyiratkan `ViewAmount`.**

Bila keduanya digabung, setiap pemegang butir baca umum — termasuk Auditor — dapat membaca
nominal gaji seluruh pegawai sekaligus. Pemisahan inilah inti keputusan ini; menggabungkannya
kembali membatalkannya.

### B.4 Keadaan sekarang, supaya tidak disalahbaca

| Aspek | Keadaan |
| --- | --- |
| Butir `: ViewAmount` | **Belum ada.** Diusulkan keputusan ini |
| Butir `: ViewAmountBulk` | **Belum ada**, dan **tidak diberikan** pada MVP |
| Daftar lintas pegawai | **Belum ada** — berstatus rencana pada kontrak API |
| Detail gaji satu pegawai | **Sudah ada**, dan sudah dijaga butir hak akses hari ini |
| Nominal pada log | Sudah dilarang; menjadi butir uji `AT-HRD-A1-05` |

Artinya keputusan ini **mendahului** pembangunannya — ia menetapkan bentuk sebelum layarnya
dibuat, bukan memperbaiki layar yang sudah bocor.

### B.5 Yang diminta dinilai keamanan

| # | Pertanyaan |
| ---: | --- |
| 1 | Apakah **`NOT_RETURNED` pada daftar lintas pegawai** diterima sebagai bentuk yang benar, bukan sekadar penyembunyian di layar? |
| 2 | Apakah daftar **metadata yang boleh tampil** sudah tepat, atau ada yang harus dikeluarkan? |
| 3 | Apakah **`: ViewAmount` terpisah dari `: Read`/`: ReadAll`** diterima? |
| 4 | Apakah **penundaan keterlihatan massal** sampai ada `: ViewAmountBulk` beserta persetujuan keamanan baru sudah tepat? |
| 5 | Apakah **HR Manager dan Payroll Officer** adalah pemegang `: ViewAmount` yang tepat, dan apakah **Auditor benar tidak boleh** memegangnya? |
| 6 | Apakah perlu **jejak audit khusus** setiap kali nominal gaji dibaca, di luar konvensi pencatatan yang berlaku sekarang? |

Pertanyaan nomor 6 adalah satu-satunya yang **belum** dijawab keputusan pemilik. Konvensi
pencatatan yang berlaku sekarang tidak mencatat permintaan `GET`, sehingga pembacaan nominal gaji
**tidak meninggalkan jejak** kecuali keamanan meminta pengecualian bernama.

### B.6 Keputusan keamanan — diisi pemilik keamanan

| Field | Isi |
| --- | --- |
| Keputusan | **`APPROVE`** |
| Perlukah jejak audit khusus untuk pembacaan nominal? | **Ya — `APPROVED — AUDIT REQUIRED`.** Pembacaan gaji dan slip gaji ditetapkan sebagai **pengecualian** terhadap konvensi project yang tidak mencatat `GET`. Aturannya `SENSITIVE_GET_MUST_BE_AUDITED`, dicatat sebagai `HRD-DEC-039` |
| Otoritas peninjau | `Project final decision authority — Security` |
| Tanggal | 2026-08-30 |

**Model gaji diperluas melampaui pertanyaan awal paket ini.** Persetujuan disertai empat
keputusan sasaran tambahan yang dicatat pada decision log bagian 28:

| ID | Isi |
| --- | --- |
| `HRD-DEC-037` | Kewenangan konfigurasi kebijakan gaji hanya pada `HR Manager`; berversi, bertanggal berlaku, dapat diaudit, riwayat tidak dihapus |
| `HRD-DEC-038` | Kepemilikan slip gaji diturunkan backend; otentikasi bertingkat memakai kata sandi Identity canonical; `SALARY_SENSITIVE_SESSION` bawaan 5 menit |
| `HRD-DEC-039` | `SENSITIVE_GET_MUST_BE_AUDITED` beserta daftar isi audit yang boleh dan yang dilarang |
| `HRD-DEC-040` | `Cache-Control: no-store`; larangan persistensi di sisi klien; unduhan slip gaji lewat endpoint terautentikasi, bukan URL statis publik |

**Satu ketidaksesuaian dicatat, tidak diputuskan sendiri.** Faktor penentu gaji "masa studi"
tidak punya padanan di source; ketiga faktor lain punya. Dicatat sebagai `HRD-Q-55` — lihat
decision log bagian 28.2.1.

---

## C. Rujukan bila diperlukan

Ketiga berkas berikut **tidak wajib dibaca** untuk menjawab, dan disebut hanya bila peninjau
ingin memeriksa lebih jauh.

| Berkas | Isi yang relevan |
| --- | --- |
| [`../contracts/permission-audit-matrix.md`](../contracts/permission-audit-matrix.md) | Bagian 2.2 usulan peta peran; 2.3 pemetaan ke peran Identity; 2.4 pemisahan peran; 2.5 keterlihatan nominal |
| [`../00-interview-decisions.md`](../00-interview-decisions.md) | Bagian 26.2 dan 26.3 — isi lengkap kedua keputusan |
| [`../data/data-dictionary.md`](../data/data-dictionary.md) | Bagian 5 — rekapitulasi seluruh kolom sensitif modul HR |

## D. Apa yang terjadi setelah keamanan menjawab

| Jawaban | Tindakan berikutnya |
| --- | --- |
| `APPROVE` pada keduanya | **Terjadi.** `HRD-DEC-032` dan `HRD-DEC-033` naik menjadi `SECURITY_APPROVED`. `contracts/permission-audit-matrix.md` dan `03-frontend-architecture.md` berpindah dari `PENDING_SECURITY_COSIGN` menjadi `READY_TO_REVIEW` |
| `REQUEST_CHANGES` | Tidak terjadi |

**Tinjauan keamanan selesai 2026-08-30.** Keempat keputusan sasaran `HRD-DEC-037` s.d.
`HRD-DEC-040` adalah **kontrak sasaran**, bukan perilaku yang sudah berjalan — tidak satu pun
sudah diimplementasikan, dan tidak satu pun boleh diimplementasikan dari dokumen ini.
