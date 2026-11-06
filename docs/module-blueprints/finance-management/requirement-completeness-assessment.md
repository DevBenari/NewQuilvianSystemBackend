# Finance Management — Penilaian Kelengkapan Requirement: Benefit RS (Pengajuan, Aktivasi, Sinkronisasi Penjamin Pasien)

| Field | Nilai |
| --- | --- |
| Assessment ID | `FIN-RCG-001` |
| Revision | `2` (menggantikan revision `1`; riwayat di bawah) |
| Blueprint ID | `FIN-BP-001` — usulan Revisi 21 (`draft`; `blueprint-manifest.md` belum diubah) |
| Status | `final` — hasil gerbang, 3 November 2026 (setelah Closure Pass `grill-me`) |
| **Hasil** | **Sembilan slice (`S1`–`S9`) berstatus `READY_FOR_DOMAIN_DESIGN`.** Tidak ada butir `BLOCKING`, tidak ada `CONFLICT`, dan tidak ada `MISSING` yang memblokir. Yang tersisa: usulan standar `NON_BLOCKING_STANDARD`, empat Open Question non-blocking (`FIN-OQ-121`, `127`, `128`, `129`), dan empat **syarat rilis** yang bukan keputusan desain (bagian 4.4) |
| Masukan | `00-interview-decisions.md` — `FIN-DEC-231`..`256`, `FIN-OQ-121`, `127`, `128`; `01-existing-capability-map.md` bagian 22 (`FIN-CAP-086`..`103`); `requirement-completeness-assessment.md` revision `1` |
| SHA backend | `ba312826` (branch `Yasmina`) — dibaca read-only; tidak berubah sejak revision `1` |
| SHA frontend | `fa7eb8d7` (branch `yasmina`) — dibaca read-only; tidak berubah sejak revision `1` |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN`. Alasan dan saran di bagian 8 |
| `indonesia-hospital-domain-reference` | Tidak dipakai. Seluruh bukti berasal dari keputusan pemilik dan source. Tidak ada gap yang butuh baseline rujukan |
| Task mode | Read-only terhadap source, database, endpoint, dan filesystem aplikasi. Hanya berkas dokumentasi blueprint yang ditulis |

### Riwayat revisi

| Revision | Hasil | Perubahan |
| --- | --- | --- |
| `1` | `PARTIALLY_READY`. `S1`, `S2`, `S3`, `S7`, `S8` siap; `S4`, `S5a`, `S5b`, `S6` terblokir `FIN-OQ-122`, `123`, `125`, `126`; `FIN-OQ-124` menentukan nasib `S9` | Penilaian pertama |
| `2` | **Seluruh `S1`–`S9` `READY_FOR_DOMAIN_DESIGN`** | Closure Pass menutup lima blocker (`FIN-DEC-250`..`256`). `S9` masuk scope dalam bentuk terbatas dan dinilai pertama kali. `FIN-GAP-005` dan `006` berubah dari `PROPOSED` menjadi `CONFIRMED`. Lima usulan standar baru (`FIN-GAP-010`..`014`) dan satu temuan source (`FIN-GAP-015`) ditambahkan |

> **Cara membaca dokumen ini.** Dokumen ini hanya menjawab satu pertanyaan: *apakah requirement tiap
> slice sudah cukup lengkap untuk dirancang?* Dokumen ini tidak merancang tabel, endpoint, atau
> layar, dan tidak menjawab keputusan bisnis atas nama pemilik.
>
> | Status bukti | Artinya |
> | --- | --- |
> | `CONFIRMED` (**C**) | Didukung keputusan pemilik yang `approved` atau source untuk keadaan sekarang |
> | `PROPOSED` (**P**) | Usulan standar agen yang **belum** diputuskan pemilik; terlihat di sini |
> | `MISSING` (**M**) | Tidak ada dalam bukti |
> | `CONFLICT` (**X**) | Dua sumber bertentangan tanpa penyelesaian |
> | **–** | Tidak berlaku secara material; alasannya ada di bagian 3 |
>
> | Dampak gap | Artinya |
> | --- | --- |
> | `BLOCKING` | Dapat mengubah model data, wewenang, integritas data, konsekuensi billing, atau alur yang sulit dibatalkan. Desain slice terdampak berhenti |
> | `NON_BLOCKING_STANDARD` | Usulan standar yang aman dinyatakan eksplisit tanpa mengubah makna inti |
> | `CONFIGURABLE_DEFAULT` | Wajar berbeda antar lingkungan dan aman dijadikan konfigurasi |

---

## 1. Scope penilaian

Scope amendment dikunci pemilik pada `FIN-DEC-231` (penuh end-to-end) dan diperluas pada `FIN-DEC-256` (`S9`, bentuk terbatas).

| Slice | Kemampuan | Keputusan inti |
| --- | --- | --- |
| `S1` | **Ajukan Benefit**: enrollment dibuat dan diajukan dalam satu operasi atomik | `FIN-DEC-243` |
| `S2` | **Setujui lalu aktifkan** dalam satu transaksi bila tanggal mulai berlaku sudah tiba; titik panggil sinkronisasi sesudah commit | `FIN-DEC-233`, `234`, `244` |
| `S3` | **Aktivasi terjadwal** untuk enrollment `Approved` yang tanggal mulainya di masa depan | `FIN-DEC-235` |
| `S4` | **Pemilik benefit hanya karyawan dan dokter tetap**; dokter tamu ditolak di semua jalur; data lama dibersihkan sebelum rilis | `FIN-DEC-240`, `247`, `254` |
| `S5a` | **Mesin sinkronisasi**: cari pasien (NIK + tanggal lahir + jenis ID KTP/NIK), pasang kartu `SYS-RS-BENEFIT`, catat hasil, ulang otomatis; kartu lain yang sudah ada ditahan | `FIN-DEC-236`, `237`, `239`, `244`, `246` (butir 1), `250`, `251` |
| `S5b` | **Visibilitas status sinkronisasi**: penanda untuk HRD, daftar kerja untuk Registrasi, alasan umum tanpa identitas pasien, "Sinkronkan ulang" | `FIN-DEC-246` (butir 2 dan 3), `255` |
| `S6` | **Cancelled mengikuti** dan **pencabutan kartu salah pasien**: kartu yang tercatat dinonaktifkan tanpa dihapus; kartu dicabut tidak dipasang ulang otomatis | `FIN-DEC-242` butir (1), `253` |
| `S7` | **Modal Daftarkan Penjamin** untuk penjamin internal dan **galat 422 yang spesifik** | `FIN-DEC-245`, `248` |
| `S8` | **Tab Penjamin Perusahaan** menampilkan status dan masa berlaku benefit langsung dari HR | `FIN-DEC-242` butir (2)–(4) |
| `S9` | **Pemeriksaan ulang identitas saat pendaftaran kunjungan**, hanya untuk kartu hasil sinkronisasi otomatis | `FIN-DEC-238`, `256` |

**Di luar penilaian** (dicatat pada `FIN-DEC-231`): perhitungan dan pemotongan plafon, Invoice, Finance AR, kwitansi, asuransi eksternal, pembuatan atau penggabungan master pasien, verifikasi tanggungan keluarga, kartu keluarga otomatis (`FIN-DEC-241`, `LATER SLICE`), pilihan kartu internal bila pasien punya lebih dari satu (`FIN-OQ-128`, `LATER SLICE`), dan perubahan schema atau migration.

## 2. Bukti yang dipakai

| Urutan wewenang | Bukti |
| --- | --- |
| 1. Requirement eksplisit dari pemilik | Permintaan awal sesi (bagian A–I) dan seluruh jawaban `grill-me` 3 November 2026 (`FIN-DEC-231`..`256`) |
| 2. Kontrak dan keputusan lama yang `approved` | `FIN-DEC-199`, `201`, `204`, `207`, `209`, `227`, `228`, `229`, `230`; `FIN-VAL-271` (digantikan `FIN-DEC-233`); klausa jamak `FIN-DEC-227` (ditangguhkan `FIN-DEC-236`) |
| 3. Bukti implementasi V2 terverifikasi (untuk "apa yang ada") | `01-existing-capability-map.md` bagian 22; backend `ba312826`, frontend `fa7eb8d7`; ditambah pembacaan source untuk `S9` (bagian 4.5) |
| 4. Bukti git | `e5fc5888` (pelebaran dokter tamu, 20 Oktober 2026) — `FIN-DEC-247` |

Tidak ada SOP tertulis, notulen rapat, atau ClickUp yang dipakai. Butir hanya `CONFIRMED` bila pemilik memilihnya secara eksplisit.

Pertentangan lama sudah tertutup: `FIN-DEC-227` vs indeks unik (`FIN-DEC-236`); `FIN-VAL-271` vs kode (`FIN-DEC-233`). **Penyelarasan dokumen kontrak** (`validation-matrix`, `acceptance-test-matrix` butir `N.2.2`, `permission-audit-matrix`) adalah tugas `design-business-module`; tidak dihitung `CONFLICT` karena keputusan pemilik yang lebih baru menang menurut waktu dan wewenang.

## 3. Matriks kelengkapan (18 dimensi × slice)

| Dimensi | S1 | S2 | S3 | S4 | S5a | S5b | S6 | S7 | S8 | S9 |
| --- | :-: | :-: | :-: | :-: | :-: | :-: | :-: | :-: | :-: | :-: |
| 01 Tujuan | C | C | C | C | C | C | C | C | C | C |
| 02 Aktor | C | C | **P** | C | C | C | C | C | C | C |
| 03 Pemicu / prasyarat | C | C | C | C | C | C | C | C | C | C |
| 04 Alur utama | C | C | C | C | C | C | C | C | C | C |
| 05 Alur alternatif / exception | C | C | **P** | C | C | C | **P** | C | **P** | C |
| 06 Data minimum | C | C | C | C | **P** | C | C | C | **P** | C |
| 07 Aturan bisnis / validasi | C | C | **P** | C | C | C | C | C | C | **P** |
| 08 Status / perubahan status | C | C | C | C | C | – | C | – | C | – |
| 09 Peran / authorization | **P** | C | C | – | C | C | C | C | **P** | – |
| 10 Dependency antarmodul | C | C | C | C | C | C | C | C | C | C |
| 11 Integrasi internal / eksternal | – | – | – | – | – | – | – | – | – | – |
| 12 Hasil akhir | C | C | C | C | C | C | C | C | C | C |
| 13 Pembatalan / koreksi | C | C | C | C | C | – | C | C | – | C |
| 14 Audit / histori | C | C | **P** | – | C | – | C | – | – | **P** |
| 15 Notifikasi | – | – | **P** | – | **P** | – | – | – | – | – |
| 16 Dampak billing / charge | – | C | C | C | C | – | C | – | – | C |
| 17 Keselamatan klinis | – | – | – | – | – | – | – | – | – | **P** |
| 18 Pelaporan / traceability | C | C | C | – | **P** | **P** | C | – | – | – |

Kolom yang berubah dari revision `1`: `S4`, `S5a`, `S5b`, `S6` (semua sel `M` menjadi `C`, beberapa `P` menjadi `C`) dan `S9` (baru). Kolom `S1`, `S2`, `S3`, `S7`, `S8` tidak berubah.

### Alasan dimensi yang "Tidak berlaku (–)"

- **11 Integrasi:** seluruh alur berada di dalam satu aplikasi (HR, Registrasi, Patient). Tidak ada sistem eksternal.
- **15 Notifikasi:** pemilik tidak meminta notifikasi dorong. Untuk `S5b` pemilik memilih **daftar kerja** yang dibuka sendiri oleh petugas Registrasi (`FIN-DEC-255`), bukan notifikasi. Untuk `S3` dan `S5a` tersisa usulan `FIN-GAP-004`.
- **16 Billing (S1, S5b, S7, S8):** tidak membuat atau mengubah konsekuensi finansial. Untuk `S2`, `S3`, `S5a`, `S6`, `S4`, dan `S9` konsekuensinya sudah ditegaskan pemilik (`FIN-DEC-244`, `242`, `254`, `256`), sehingga `C`.
- **17 Keselamatan klinis:** kartu penjamin adalah alat pembayaran, bukan data klinis. **Risiko identitas** (kartu terpasang ke pasien yang salah) adalah risiko finansial dan integritas data; ia dinilai pada dimensi 06, 07, dan 13. Satu pengecualian ada pada `S9` (dimensi 17 `P`): menolak kartu saat pendaftaran **tidak boleh menunda pelayanan darurat** (`FIN-GAP-013`).
- **08 Status (S5b, S7, S9):** `S5b` hanya menampilkan status milik `S5a`; `S7` tidak punya lifecycle; `S9` hanya membaca dan menolak, tidak mengubah status (`FIN-DEC-256`).
- **09 Authorization (S4, S9):** tidak ada izin baru; perubahan berada pada aturan kebijakan atau validasi yang sudah berizin.

## 4. Temuan kelengkapan

### 4.1 Butir `PROPOSED` (usulan agen, belum diputuskan pemilik)

Semuanya `NON_BLOCKING_STANDARD`. Butir ini harus dinyatakan eksplisit pada desain dan disetujui pemilik saat persetujuan desain; bukan kebijakan terkonfirmasi.

| ID | Slice / dimensi | Usulan baseline |
| --- | --- | --- |
| `FIN-GAP-001` | `S1` / 09 | Endpoint gabungan "Ajukan" mewajibkan pemanggil memegang **kedua** izin `EmployeeBenefitEnrollment : Create` dan `: Submit` |
| `FIN-GAP-002` | `S3` / 02, 14 | Aktivasi terjadwal dicatat sebagai tindakan **akun layanan sistem** pada `ActivatedByUserId` (mengikuti opsi `SystemActorUserId` penjadwal Finance), bukan pengguna manusia |
| `FIN-GAP-003` | `S3` / 05, 07 | `FIN-OQ-127`: bila seluruh periode enrollment `Approved` sudah lewat sebelum sempat diaktifkan, penjadwal **tidak mengaktifkannya**, mencatat galat, dan enrollment tetap `Approved` untuk dibatalkan HRD manual. Tanpa status baru |
| `FIN-GAP-004` | `S3`, `S5a` / 15 | Tanpa notifikasi dorong. Kegagalan penjadwal dan sinkronisasi terlihat sebagai penanda (`S5b`) dan dicatat di log tanpa data pribadi. Penjadwal mengikuti pola Finance: nonaktif bawaan sehingga perlu dinyalakan lewat konfigurasi (syarat rilis, bagian 4.4) |
| `FIN-GAP-007` | `S5a`, `S5b` / 18 | Penanda sinkronisasi pada daftar enrollment dapat disaring per status, sehingga HRD menghitung "berapa yang menunggu, ditahan, atau dicabut" |
| `FIN-GAP-008` | `S7` / 05 | Setiap keadaan pada `FIN-DEC-245` butir (6) mendapat pesan Indonesia khusus. Fakta source: pesan sekarang menyebut status dengan nama Inggris (`Draft`, `Submitted`, `Approved`) dan **tidak membedakan** "belum mulai" dari "sudah berakhir"; kode alasan tidak ikut pada respons galat. Usulan: pesan per keadaan, kode alasan terbaca mesin sebagai opsi |
| `FIN-GAP-009` | `S8` / 05, 06, 09 | Tab membaca HR secara fail-closed: bila HR tidak dapat dibaca, kartu internal tampil "status benefit tidak dapat dibaca", bukan aktif. Respons tanpa saldo plafon. Jumlah kartu per halaman dibatasi agar beban baca HR terkendali. Hak baca status HR mengikuti hak baca tab |
| `FIN-GAP-010` | `S9` / 07 | **Kontrak galat per jalur.** Jalur ganti penanggung (`EncounterPaymentSourceService`) sudah mengembalikan **422 dengan kode alasan**. Jalur buat kunjungan di loket (`PatientEncounterController`) mengembalikan **400 hanya dengan pesan** untuk semua kegagalan kartu. `FIN-DEC-256` menyebut 422 + `PATIENT_IDENTITY_MISMATCH`. Usulan: 422 dengan kode pada jalur ganti penanggung; pada jalur loket pertahankan envelope 400 yang ada dengan pesan khusus, kecuali pemilik meminta disamakan (menyamakan mengubah penanganan galat Frontend loket). **Pemilik menyetujui salah satu saat persetujuan desain** |
| `FIN-GAP-011` | `S5a`, `S9` / 06, 07 | Perbandingan NIK memakai **angka saja pada kedua sisi** (membuang spasi, titik, tanda hubung), supaya varian format tidak menghasilkan false negative. Perlu tidaknya ditentukan hasil pemeriksaan data nyata oleh pemilik (syarat rilis). Tanggal lahir dibandingkan sebagai tanggal kalender (kolom `date` di ketiga tabel) |
| `FIN-GAP-012` | `S9` / 14 | Setiap penolakan dicatat pada log audit: pelaku, kunjungan atau kartu, alasan umum, **tanpa NIK dan nama** (`QBE-LOG-001`) |
| `FIN-GAP-013` | `S9` / 17 | Pemeriksaan hanya **menolak kartu**, bukan pendaftaran. Pendaftaran IGD tetap dapat dilanjutkan sebagai Pasien Umum atau penjamin lain; pesan galat mengarahkan petugas ke sana |
| `FIN-GAP-014` | `S6` / 05 | Catatan hasil sinkronisasi atas enrollment yang `Cancelled` sebelum sempat berhasil (masih "menunggu", "ditahan", atau "gagal sementara") **dihentikan**: penjadwal tidak memprosesnya lagi, tanpa status baru |
| `FIN-GAP-015` | `S4` / 06 | Temuan source (`FIN-CAP-091`): pencarian penerima kehilangan filter `IsCancel` dan, bila profil workforce tidak ada, mengisi `WorkforceProfileId` dengan ID peran (`EmployeeId`/`DoctorId`). Usulan perbaikan: saring SDM yang batal dan jangan pernah mengisi ID pemilik dengan ID peran. Sejalan dengan prinsip permintaan awal "tidak menciptakan identitas palsu" |

**Berubah dari revision 1:** `FIN-GAP-005` (jenis ID KTP/NIK) kini `CONFIRMED` oleh `FIN-DEC-250`; `FIN-GAP-006` (status "dicabut") kini `CONFIRMED` oleh `FIN-DEC-253`. Keduanya tidak lagi tercantum di tabel di atas.

### 4.2 Butir `MISSING` yang memblokir

**Tidak ada.** Lima butir yang memblokir pada revision `1` seluruhnya tertutup:

| Decision ID | Ditutup oleh |
| --- | --- |
| `FIN-OQ-122` | `FIN-DEC-250` |
| `FIN-OQ-123` | `FIN-DEC-254` |
| `FIN-OQ-124` | `FIN-DEC-256` |
| `FIN-OQ-125` | `FIN-DEC-255` |
| `FIN-OQ-126` | `FIN-DEC-251` dan `FIN-DEC-253` |

### 4.3 `CONFLICT`

**Tidak ada `CONFLICT` yang tersisa.** Catatan: `FIN-DEC-256` menyebut status 422, sedangkan jalur loket sekarang 400 dengan pesan saja. Itu **bukan** pertentangan antar keputusan; ia perbedaan kontrak dengan source dan dicatat sebagai usulan standar `FIN-GAP-010`.

### 4.4 Syarat rilis (bukan keputusan desain)

Pekerjaan pemilik atau tim di luar sistem. Tidak menghalangi desain, tetapi menghalangi rilis slice terkait.

| ID | Syarat | Slice | Dasar |
| --- | --- | --- | --- |
| `R-1` | Pemilik HR memeriksa data nyata dan membersihkan enrollment serta kunjungan dokter tamu; kasir menyelesaikan dulu kunjungan yang belum final | `S4` | `FIN-DEC-254` |
| `R-2` | Pemilik memeriksa apakah NIK pasien atau HR memuat spasi, titik, atau tanda hubung, lalu menyerahkan hasilnya | `S5a`, `S9` | `FIN-GAP-011` |
| `R-3` | Pemilik modul dan prefix tabel catatan hasil sinkronisasi terdaftar di `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` sebelum model dibuat; tabel dan migration dibuat oleh tim pengembang | `S5a`, `S5b`, `S6`, `S9` | `FIN-OQ-121`, `FIN-DEC-239`, `QBE-MOD-002` |
| `R-4` | Penjadwal aktivasi dan penjadwal sinkronisasi dinyalakan lewat konfigurasi (bawaan pola Finance: nonaktif), dan akun layanan sistem ditetapkan | `S3`, `S5a` | `FIN-GAP-002`, `FIN-GAP-004` |

### 4.5 Fakta source untuk `S9`

| Fakta | Bukti |
| --- | --- |
| Hanya **dua jalur** yang menempelkan kartu internal ke kunjungan dan keduanya sudah memanggil `ValidateCardForServiceAsync`: (1) buat kunjungan di loket, (2) ganti penanggung kunjungan | `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs` `LoadValidPatientCompanyGuarantorAsync` (sekitar baris 2764; dipanggil pada baris 1031 dan 2448); `…/Services/EncounterPaymentSourceService.cs:459` |
| Penagihan kasir (`BillingPayerEditService`, memanggil `SwitchPaymentSourceAsync` pada baris 562) dan transfer rawat inap (`InpAdmissionTransferService.cs:135` memanggil `InitializeAdmissionAsync`, yang untuk pembayaran non-tunai berakhir di `SwitchPaymentSourceAsync` pada `EncounterPaymentSourceService.cs:69`) **bermuara ke jalur (2)**, sehingga satu perubahan di jalur (2) mencakup keduanya | `…/Billing/Services/BillingPayerEditService.cs:30,562`; `…/InPatientManagement/Services/InpAdmissionTransferService.cs:135`; `…/EncounterPaymentSourceService.cs:33-69` |
| **Batas yang tidak dicakup `S9`:** kunjungan yang **mewarisi** pembayaran dari kunjungan lain (cabang `inheritedPaymentPreview`, misalnya bayi baru lahir yang memakai penanggung ibu) tidak memanggil `LoadValidPatientCompanyGuarantorAsync`. Pasien kunjungan itu bukan pemilik kartu, jadi pencocokan identitas pemilik tidak bermakna di sana. Layanan pewarisan (`NewbornEncounterPaymentInheritanceService`) **belum saya telusuri** | `PatientEncounterController.cs:1029`; `FIN-DEC-201`, `204`; dicatat sebagai `FIN-OQ-129` (di luar scope) |
| Pendaftaran kiosk dan check-in MCU **tidak** dapat memakai kartu internal: `EncounterIntakeService` menolaknya ("hanya dapat dipilih lewat pendaftaran loket") | `…/RegistrationManagement/Services/EncounterIntakeService.cs:655-660` |
| Jalur (2) mengembalikan 422 dengan `ErrorCode`; jalur (1) mengembalikan 400 dengan pesan saja | `EncounterPaymentSourceService.cs:459-475`; `PatientEncounterController.cs:1036-1041` |

## 5. Decision Log

Tidak ada entri `BLOCKING` yang terbuka. Open Question yang masih berstatus `OPEN` seluruhnya non-blocking:

| ID | Pertanyaan | Dampak gap | Pemilik | Status | Dampak implementasi / domain |
| --- | --- | --- | --- | --- | --- |
| `FIN-OQ-121` | Pemilik modul dan prefix tabel catatan hasil sinkronisasi | `NON_BLOCKING_STANDARD` untuk desain | Finance, HR, Registrasi | `OPEN` | Diusulkan `design-business-module`, disetujui pemilik registry. **Gerbang implementasi** (`R-3`). Fakta pembatas: HR BenefitManagement tidak terdaftar di registry; `Pat` dan `Fin` sudah `ACTIVE` |
| `FIN-OQ-127` | Enrollment `Approved` yang seluruh periodenya sudah lewat sebelum diaktifkan | `NON_BLOCKING_STANDARD` | HR | `OPEN` (usulan `FIN-GAP-003`) | Tidak menghalangi desain `S3` |
| `FIN-OQ-128` | Kartu internal mana yang dipakai bila pasien punya lebih dari satu | Di luar scope (`LATER SLICE`) | HR dan Registrasi | `OPEN` | Tidak berdampak pada slice amendment ini. `FIN-DEC-251` menahan pemasangan otomatis sehingga sinkronisasi tidak menciptakan keadaan itu |
| `FIN-OQ-129` | Apakah kunjungan bayi baru lahir yang mewarisi pembayaran ibu memverifikasi tanggungan HR sebelum mewarisi kartu internal? (`FIN-DEC-201`, `204`) | Di luar scope (`LATER SLICE`) | HR dan Registrasi | `OPEN` | Perilaku sudah ada sebelum amendment dan tidak diubah olehnya; layanan pewarisan belum ditelusuri, sehingga ia butuh `trace-existing-capabilities` sebelum diputuskan |

## 6. Kesiapan per slice

| Slice | Kesiapan | Alasan | Butir yang menyertai |
| --- | --- | --- | --- |
| `S1` Ajukan Benefit | **`READY_FOR_DOMAIN_DESIGN`** | Semua dimensi `C`, kecuali izin (`P`) | `FIN-GAP-001` |
| `S2` Setujui + aktifkan | **`READY_FOR_DOMAIN_DESIGN`** | Aturan aktivasi, kegagalan atomik, izin, dan audit dikunci. Titik panggil sinkronisasi didefinisikan sebagai antarmuka | `FIN-DEC-234`, `244`. Respons memuat status sinkronisasi dari `S5a` |
| `S3` Aktivasi terjadwal | **`READY_FOR_DOMAIN_DESIGN`** | Perilaku utama dikunci; sisa butir adalah usulan standar | `FIN-GAP-002`, `003`, `004`; `R-4` |
| `S4` Dokter tamu | **`READY_FOR_DOMAIN_DESIGN`** *(baru; sebelumnya `BUSINESS_DECISION_REQUIRED`)* | `FIN-DEC-254` mengunci penerapan menyeluruh dan pembersihan data | `FIN-GAP-015`; `R-1`. Resolver dipakai Enrollment, Registrasi, Billing, dan Finance, jadi desain wajib memuat strategi uji lintas modul |
| `S5a` Mesin sinkronisasi | **`READY_FOR_DOMAIN_DESIGN`** *(baru)* | Kriteria identitas (`FIN-DEC-250`), aturan kartu yang ada (`251`), pencabutan (`253`), dan lima status (`239`, `253`) dikunci | `FIN-GAP-004`, `007`, `011`; `R-2`, `R-3`, `R-4` |
| `S5b` Visibilitas sinkronisasi | **`READY_FOR_DOMAIN_DESIGN`** *(baru)* | `FIN-DEC-255` mengunci siapa melihat apa; bergantung pada kontrak status dari `S5a` | `FIN-GAP-007`; `R-3` |
| `S6` Cancelled dan pencabutan | **`READY_FOR_DOMAIN_DESIGN`** *(baru)* | `FIN-DEC-242` dan `253` dikunci; bergantung pada catatan hasil dari `S5a` | `FIN-GAP-014`; `R-3` |
| `S7` Modal + galat 422 | **`READY_FOR_DOMAIN_DESIGN`** | Perilaku dikunci (`FIN-DEC-245`); sebab galat sudah dibuktikan (`FIN-DEC-248`). Kartu manual untuk kasus "ditahan" dikenali `S5a` lewat aturan pakai ulang `FIN-DEC-251` | `FIN-GAP-008` |
| `S8` Tab Penjamin Perusahaan | **`READY_FOR_DOMAIN_DESIGN`** | Aturan baca langsung dari HR dikunci (`FIN-DEC-242`); tidak bergantung pada catatan sinkronisasi | `FIN-GAP-009` |
| `S9` Pemeriksaan ulang saat kunjungan | **`READY_FOR_DOMAIN_DESIGN`** *(baru dinilai)* | `FIN-DEC-256` mengunci cakupan (kartu hasil sinkronisasi saja), kriteria (NIK, tanggal lahir, jenis ID), dan hasil (tolak, tidak ubah status). Hanya dua jalur yang perlu diubah (bagian 4.5). Bergantung pada catatan hasil `S5a` untuk mengenali kartu otomatis | `FIN-GAP-010`, `011`, `012`, `013`; `R-2`, `R-3` |

**Kesiapan keseluruhan: `READY_FOR_DOMAIN_DESIGN` untuk `S1`–`S9`.**

## 7. Apa yang boleh berjalan dan apa yang harus berhenti

### Boleh berjalan

Seluruh slice dapat masuk desain. Urutan yang disarankan mengikuti dependency:

```
S1 ──► S2 ──► [titik panggil sinkronisasi] ──► S5a ──► S5b
              │                                  │  ├──► S6
              └──► S3                            │  └──► S9
S4  (mengubah resolver pemilik yang dipakai S1, S2, S3, S5a, S9; diuji lintas modul)
S7, S8 berdiri sendiri (membaca kartu dan HR yang sudah ada)
```

- `S1`, `S2`, `S3`, `S7`, `S8`, `S4` tidak bergantung pada `S5a` dan dapat didesain lebih dulu atau paralel.
- `S5a` harus didesain sebelum `S5b`, `S6`, dan `S9`, karena ketiganya memakai catatan hasil sinkronisasi (kontrak status, ID kartu, penanda "kartu hasil sinkronisasi").
- Desain `S2` harus mendefinisikan titik panggil sinkronisasi dan bentuk status sinkronisasi pada respons sebagai kontrak, supaya `S5a` dapat dirancang tanpa mengubah `S2`.

### Harus berhenti

**Tidak ada slice yang harus berhenti karena keputusan bisnis.** Yang menahan adalah syarat rilis pada bagian 4.4 (`R-1`..`R-4`), bukan desain.

## 8. `domain_architecture_readiness`

`DOMAIN_ARCHITECTURE_NOT_RUN`.

- **Disarankan** `hospital-domain-architect` untuk **`S5a`, `S5b`, `S6`, `S9`**: melintasi HR, Registrasi/Patient, penjamin bersama (`SYS-RS-BENEFIT`), dan berdampak ke Billing. Pertanyaan arsitektur domain yang sudah terlihat: pemilik catatan hasil sinkronisasi (`FIN-OQ-121`); arah dependensi (Registrasi sudah bergantung pada layanan HR, sehingga pemicu dari HR sebaiknya antarmuka atau event, bukan pemanggilan langsung ke sisi Patient); dan bagaimana `S9` mengenali kartu hasil sinkronisasi tanpa menambah kolom pada kartu.
- **Opsional** untuk `S4`: satu bounded context, tetapi dipakai bersama banyak modul sehingga strategi uji lintas modul wajib ada pada desain.
- **Tidak diperlukan** untuk `S1`, `S2`, `S3`, `S7`, `S8`.

## 9. Keputusan pemilik yang dibutuhkan

Tidak ada keputusan bisnis yang memblokir. Persetujuan yang tetap dibutuhkan pada saat persetujuan desain:

| No | Butir | Slice |
| ---: | --- | --- |
| 1 | `FIN-GAP-010`: samakan galat jalur loket dengan jalur ganti penanggung (422 + kode), atau pertahankan envelope 400 loket | `S9` |
| 2 | `FIN-OQ-127` / `FIN-GAP-003`: perlakuan enrollment `Approved` yang periodenya sudah lewat | `S3` |
| 3 | `FIN-OQ-121`: pemilik modul dan prefix tabel (pemilik registry) | `S5a`, `S5b`, `S6`, `S9` |
| 4 | Usulan standar lain (`FIN-GAP-001`, `002`, `004`, `007`, `008`, `009`, `011`..`015`) disetujui bersama dokumen desain | semua |

Tindakan pemilik di luar sistem: `R-1`, `R-2`, `R-3` (bagian 4.4).

## 10. Handoff

| Ke | Untuk |
| --- | --- |
| **`hospital-domain-architect`** | `S5a`, `S5b`, `S6`, `S9` (disarankan), opsional `S4` |
| **`design-business-module`** | Seluruh `S1`–`S9`. `S1`, `S2`, `S3`, `S7`, `S8` dapat dimulai sekarang tanpa menunggu arsitektur domain |
| `grill-me` | Tidak diperlukan sekarang. Hanya bila `FIN-GAP-010` perlu keputusan lebih awal |
| `trace-existing-capabilities` | Tidak perlu. Capability map bagian 22 masih valid pada `ba312826` / `fa7eb8d7`; bukti tambahan `S9` ada di bagian 4.5 |

Yang dipertahankan pada handoff: Modul Finance Management, `FIN-BP-001` usulan Revisi 21, bukti `FIN-DEC-231`..`256`, capability map bagian 22, klasifikasi pada bagian 3–4, Decision ID `FIN-OQ-121`, `127`, `128`, `129`, usulan standar `FIN-GAP-001`..`015`, dan syarat rilis `R-1`..`R-4`.
