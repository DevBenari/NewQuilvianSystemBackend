# Acceptance Test Matrix — Modul IGD

| Field | Nilai |
| --- | --- |
| Blueprint | `IGD-BP-001` revision `4` |
| Commit diaudit | backend `e5331a0` |

Matriks ini memuat jalur gagal, bukan hanya jalur berhasil. Skenario yang hanya menguji jalur
berhasil tidak membuktikan modul siap dipakai.

---

## 1. Kunjungan dan pasien tidak dikenal

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-IGD-001` | `IGD-DEC-046` | Membuat kunjungan IGD dengan pasien terdaftar | Integration | Kunjungan tersimpan, nomor terbentuk, `EncounterId` terisi |
| `AT-IGD-002` | Jalur keselamatan | Membuat kunjungan untuk pasien tidak dikenal tanpa `PatientId` | Integration | Diterima, `IsUnknownPatient` bernilai benar |
| `AT-IGD-003` | Jalur keselamatan | Pasien gawat mulai ditangani sebelum registrasi selesai | Integration | Status berpindah ke `InTreatment` walau registrasi `Provisional` |
| `AT-IGD-004` | Validasi | Membuat kunjungan tanpa pasien dan tanpa penanda tidak dikenal | Integration, **jalur gagal** | Ditolak 400 dengan pesan yang dapat dibaca petugas |
| `AT-IGD-005` | Integritas | Membuat kunjungan kedua untuk `EncounterId` yang sama | Integration, **jalur gagal** | Ditolak 409 |

---

## 2. Triage dan retriage

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-IGD-010` | `IGD-DEC-047`, `IGD-DEC-048` | Menilai pasien pada level 1 | Integration | `TriageLevelId` terisi, `MaxWaitingMinutesSnapshot` tersalin dari master |
| `AT-IGD-011` | Perhitungan target | `ResponseDueAt` dihitung server dari `StartedAt` dan target master | Unit | `ResponseDueAt` sama dengan `StartedAt` ditambah `MaxWaitingMinutes` |
| `AT-IGD-012` | Target belum dikonfigurasi | Menilai pada level yang `MaxWaitingMinutes`-nya belum disetel | Unit, **jalur batas** | `ResponseDueAt` kosong; pasien tidak dihitung melampaui batas |
| `AT-IGD-013` | `IGD-DEC-048` | Retriage atas penilaian berstatus `Completed` | Integration | Baris baru dibuat, `PreviousTriageId` terisi, baris lama menjadi `Superseded` |
| `AT-IGD-014` | Integritas riwayat | Retriage tidak menimpa baris lama | Integration | Jumlah baris triage bertambah, isi baris lama tidak berubah |
| `AT-IGD-015` | Validasi | Retriage atas penilaian berstatus `Cancelled` | Integration, **jalur gagal** | Ditolak 409 dengan pesan yang dapat dibaca perawat |
| `AT-IGD-016` | Validasi | Menyelesaikan penilaian tanpa level triage | Integration, **jalur gagal** | Ditolak 400 |
| `AT-IGD-017` | Keselamatan | Aplikasi mencoba menetapkan kategori Hitam secara otomatis | Unit, **jalur gagal** | Ditolak; kategori Hitam hanya oleh klinisi berwenang |

---

## 3. Pemantauan SLA

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-IGD-020` | `IGD-GAP-007` | Pasien melewati `ResponseDueAt` dan belum ditangani | Integration | `IsSlaBreached` menjadi benar, `SlaBreachedAt` terisi |
| `AT-IGD-021` | Batas presisi | Pasien pada 1 menit sebelum `ResponseDueAt` | Integration, **jalur batas** | Tidak ditandai breach |
| `AT-IGD-022` | Idempotensi | Pemindaian dijalankan dua kali berturut-turut | Integration | Penandaan tidak berganda, `SlaBreachedAt` tidak berubah |
| `AT-IGD-023` | Tidak memblokir | Pemindaian gagal karena kesalahan tak terduga | Integration, **jalur gagal** | Triage, penanganan, dan penyelesaian kunjungan tetap berjalan |
| `AT-IGD-024` | Daftar breach | Mengambil daftar pasien yang melampaui batas | Integration | Hanya memuat pasien yang benar-benar melampaui dan belum ditangani |

---

## 4. Penyelesaian kunjungan

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-IGD-030` | `IGD-DEC-049` | Menyelesaikan kunjungan dari status `Disposed` | Integration | Status menjadi `Completed`, `VisitCompletedAt` terisi |
| `AT-IGD-031` | `IGD-DEC-021` | Menyelesaikan kunjungan saat billing masih `Outstanding` | Integration | **Diterima**; billing bukan syarat penyelesaian klinis |
| `AT-IGD-032` | Closure gate | Menyelesaikan kunjungan saat masih ada observasi `Active` | Integration, **jalur gagal** | Ditolak 409 |
| `AT-IGD-033` | Closure gate | Menyelesaikan kunjungan saat transfer belum tuntas | Integration, **jalur gagal** | Ditolak 409 |
| `AT-IGD-034` | Transisi tidak sah | Menyelesaikan kunjungan dari status `InTreatment` | Integration, **jalur gagal** | Ditolak 409 |
| `AT-IGD-035` | Finalitas | Mengubah status setelah `Completed` | Integration, **jalur gagal** | Ditolak 409 |

---

## 5. Transfer dan pemisahan tugas

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-IGD-040` | Alur transfer | Rangkaian `Requested`, `Accepted`, `InTransit`, `Completed` | Integration | Seluruh transisi tercatat beserta pelakunya |
| `AT-IGD-041` | Pemisahan tugas | Pengaju transfer mencoba menerima transfernya sendiri | Integration, **jalur gagal** | Ditolak 403 |
| `AT-IGD-042` | Penolakan | Unit tujuan menolak tanpa mengisi alasan | Integration, **jalur gagal** | Ditolak 400 |

---

## 6. Hak akses

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-IGD-050` | Authorization | Pengguna tanpa policy mengakses endpoint triage | Integration, **jalur gagal** | Ditolak 403 |
| `AT-IGD-051` | Authorization | Pengguna belum masuk | Integration, **jalur gagal** | Ditolak 401 |
| `AT-IGD-052` | `IGD-DEC-050` | SuperAdmin mengakses endpoint klinis tanpa policy | Integration, **jalur gagal** | **Ditolak 403** setelah pemisahan kewenangan diterapkan |
| `AT-IGD-053` | `IGD-DEC-050` | SuperAdmin mengakses endpoint `IsSystemOnly` | Integration | Diterima; wilayah teknis tetap terbuka |
| `AT-IGD-054` | Masa berlaku | Pengguna dengan penugasan organisasi yang sudah lewat masa berlakunya | Integration, **jalur gagal** | Ditolak 403 |

---

## 7. Privasi dan audit

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-IGD-060` | Privasi | Membuat dan mengubah triage, lalu memeriksa isi log | Integration | Log hanya memuat `EntityId`, controller, action, status |
| `AT-IGD-061` | Privasi | Memeriksa log setelah mengisi ringkasan klinis | Integration, **jalur gagal bila melanggar** | Tidak ada keluhan, diagnosis, maupun ringkasan klinis di log |
| `AT-IGD-062` | Audit | Menandai data terhapus | Integration | Baris tetap ada dengan `IsDelete` benar dan `DeleteBy` terisi |
| `AT-IGD-063` | Antarmuka | Menampilkan daftar triage | Component | Nama pasien tampil sebagai nama, bukan UUID |

---

## 8. Master data

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-IGD-070` | Data awal | Menjalankan modul dengan master triage kosong | Integration, **jalur gagal** | Penilaian triage tidak dapat dibuat; pesan mengarahkan pengisian master |
| `AT-IGD-071` | Konfigurasi | Mengubah `MaxWaitingMinutes` pada master | Integration | Penilaian lama tidak berubah karena memakai snapshot |
| `AT-IGD-072` | Integritas | Menghapus level triage yang sudah dipakai | Integration, **jalur gagal** | Ditolak 409 dengan saran menonaktifkan |
| `AT-IGD-073` | Setting | Menandai setting kedua sebagai default | Integration, **jalur gagal** | Ditolak 409 |

---

## Cakupan yang belum dapat diuji

| Area | Alasan |
| --- | --- |
| Target waktu level 2 sampai 5 | `MaxWaitingMinutes` belum dikonfigurasi karena SOP MMC belum tersedia |
| Break-glass akses darurat | Mekanismenya belum ada di kode |
| Scope resource dan unit pada authorization | Belum ada parameter resource pada pemeriksaan akses |

Ketiganya **tidak** boleh dianggap lulus hanya karena tidak diuji.
