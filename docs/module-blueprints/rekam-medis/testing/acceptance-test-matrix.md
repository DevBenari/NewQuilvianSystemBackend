# Acceptance Test Matrix — Rekam Medis Existing Clinical Foundation

| Field | Nilai |
| --- | --- |
| `contract_version` | `rm-existing-clinical-acceptance-v0.2-draft` |
| Status | `draft` |
| Input | Gate revision `4`; domain architecture revision `1`; backend `5103e68`; frontend `c4e2ef2a6` |
| Approval | Belum tersedia — `RM-APR-002` |
| Scope | Verifikasi reuse/adapter dan guardrail; bukan acceptance fitur RM yang belum ada |

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| Ownership pasien | Buka patient valid dari doctor workspace | Contract/integration | Data berasal dari Patient Management; tidak ada master pasien RM. |
| Ownership encounter | Buka antrean/konsultasi valid | Contract/integration | Encounter/queue berasal dari Registration. |
| Konsistensi konteks | Patient dan encounter cocok | Unit/integration | Assessment, konsultasi, dan fakta terkait dapat dimuat. |
| Konsistensi konteks | Encounter milik pasien lain | Negative safety/security | `409/403`; SOAP dan fakta klinis tidak ditampilkan. |
| Assessment existing | Membuat dan complete assessment valid | API integration | Status owner berubah sesuai enum; belum dihitung sebagai signature RM. |
| Assessment finality | PUT assessment yang sudah terkunci finality RM | Negative contract target | Ditolak; pengguna diarahkan ke correction/addendum. |
| SOAP existing | Membuat konsultasi dan mengisi SOAP | API/UI integration | Data tersimpan pada `TrxDoctorConsultation`, bukan salinan RM. |
| Finalization validation | SOAP atau diagnosis utama kurang | Negative API/UI | Complete ditolak dan daftar kekurangan ditampilkan. |
| Atomic finalization | Finalisasi resep/consultation gagal di tengah transaksi | Transaction integration | Semua perubahan transaksi existing di-rollback; tidak ada status setengah selesai. |
| Concurrency finalization | `ExpectedUpdatedAt` stale | Negative concurrency | `409`; pengguna diminta memuat ulang. |
| Diagnosis utama | Menetapkan satu diagnosis utama | API integration | Tepat satu diagnosis aktif menjadi primary. |
| Diagnosis utama | Dua request set-primary bersaing | Concurrency/negative | Tidak terbentuk dua primary; konflik ditangani. |
| Tindakan conditional | Tindakan baru masih `Planned` | Domain mapping | Item wajib tindakan belum dianggap selesai. |
| Tindakan conditional | Tindakan benar-benar dieksekusi | Domain mapping | Event nyata kelak menambah item conditional dari snapshot policy. |
| Pembatalan pemicu | Tindakan pemicu dibatalkan | Negative governance | Item tidak hilang otomatis; menunggu alasan/review/pengesahan. |
| Alergi keselamatan | Membuka active alerts | API/UI integration | Alert berasal dari owner dan tidak diduplikasi. |
| Alergi high risk | Pengguna mencoba delete fakta resmi | Negative safety | Ditolak oleh guard target; percobaan diaudit tanpa isi klinis. |
| Vital kritis | Critical alert tersedia | API/UI integration | Alert ditampilkan dan acknowledgement owner dapat ditelusuri. |
| CPPT reuse | Membuat CPPT dari konsultasi | API integration | `SourceReferenceId`/provenance dipertahankan. |
| CPPT immutable | PUT/DELETE CPPT signed | Negative finality | Ditolak; versi lama tetap ada. |
| Dokumen klinis | Membaca dokumen dengan hash | Contract | Owner ID, status, file hash, dan waktu versi tersedia. |
| Dokumen final | PUT/DELETE dokumen approved/signed | Negative finality | Ditolak; correction/entered-in-error diperlukan. |
| Consent existing | Sign consent valid | API integration | Status owner menjadi `Signed`; bukti RM tetap “belum lengkap” bila reauth/hash/meaning tidak tersedia. |
| Consent immutable | Mengubah consent signed | Negative finality | Ditolak oleh extension target. |
| Permission existing | User mempunyai `Read` dan assignment aktif | Authorization target | Akses sesuai scope diperbolehkan. |
| Contextual authorization | User mempunyai `Read`, tanpa assignment aktif | Negative authorization | Akses ditolak; permission generic saja tidak cukup. |
| Sensitive access | Dokumen kategori sensitif dibuka normal | Negative privacy | Tersembunyi; break-glass masih fail-closed. |
| Break-glass disabled | Policy/approval belum lengkap | Negative configuration | Aktivasi ditolak dan pesan konfigurasi ditampilkan. |
| Release disabled | Evidence policy pemohon belum approved | Negative configuration | Pelepasan tidak dapat dimulai. |
| Duplicate submit | Key dan payload sama dikirim ulang | Idempotency target | Hasil pertama dikembalikan; tidak ada record kedua. |
| Idempotency conflict | Key sama, payload berbeda | Negative integration | `409` dan reconciliation case. |
| Owner unavailable | Lookup patient/encounter timeout | Resilience/UI | Error dibedakan dari empty; data pasien sebelumnya dibersihkan. |
| Partial failure | Record completed, event RM gagal | Resilience | Record owner tetap sah; status sinkronisasi tertunda dan retry durable. |
| Logging sensitif | Mutation atau error terjadi | Security/logging | Log hanya metadata; tidak memuat MRN, SOAP, diagnosis, nilai vital, signer, file path. |
| Existing-first UI | Doctor queue dibuka | UI regression | Route/tab existing tetap dipakai; tidak muncul menu RM baru. |
| Deferred menu | Pengguna mencari break-glass/release/worklist RM | Scope test | Fitur tidak tersedia dan tidak dibuat diam-diam. |
| No migration | Revision blueprint diterapkan | Static/repository | Tidak ada model/configuration/migration/seed baru. |

## Jalur end-to-end existing-first

1. Petugas membuka antrean dokter existing untuk patient dan encounter yang cocok.
2. Assessment dan vital existing dimuat.
3. Dokter mengisi SOAP, diagnosis utama, tindakan bila ada, dan resep melalui owner masing-masing.
4. Finalization validation menolak bila isian owner yang diwajibkan existing belum lengkap.
5. Complete konsultasi berjalan atomik pada service existing.
6. Hasil `Completed` hanya menjadi bukti provider; episode/signature RM belum diklaim tersedia.

**Hasil akhir:** kemampuan yang sudah ada dapat dikembangkan lebih dulu tanpa menggandakan data.
Gap finality, contextual access, audit, checklist RM, dan menu yang belum ada tetap terlihat sebagai
acceptance yang harus ditutup pada slice extension berikutnya.
