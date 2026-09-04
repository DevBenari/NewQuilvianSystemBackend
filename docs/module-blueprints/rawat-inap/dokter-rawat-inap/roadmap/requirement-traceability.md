# Requirement Traceability — Sub-modul `dokter-rawat-inap` (Rawat Inap)

## Metadata

```yaml
module_id: rawat-inap
submodule: dokter-rawat-inap
traceability_revision: 1
status: APPROVED
blueprint_shape: COMPOSITE
blueprint_root: docs/module-blueprints/rawat-inap/dokter-rawat-inap/
approved_by:
  - "Muhammad Hamzah — Product/Domain owner (RWI-DEC-061), approval desain 2026-09-03"
approved_at: "2026-09-03"
contract_versions: "0.3.0"
source_sha:
  backend: "93b3227c431401d8f586dec4e1fb25fbf41766e3"
  frontend: "863f24b0d1617069310c04e5770b47fd1b518b5b"
roadmaps:
  backend: "roadmap/backend-roadmap.md revision 1"
  frontend: "roadmap/frontend-roadmap.md revision 1"
task_range:
  backend: "BE-RWI-037 s.d. BE-RWI-053"
  frontend: "FE-RWI-042 s.d. FE-RWI-050"
```

---

## 0. Cara memakai dokumen ini

Dokumen ini menjawab empat pertanyaan yang sering ditanyakan saat pekerjaan berjalan:

| Pertanyaan | Bagian |
| --- | --- |
| Kemampuan ini dikerjakan task yang mana? | 1 |
| Task ini membuktikan requirement yang mana, dan diuji apa? | 2 |
| Keputusan pemilik ini diwujudkan di mana? | 3 |
| Apa yang **belum** punya task atau belum punya test? | 4 |

Seluruh task berstatus **`BELUM DIKERJAKAN`**. Kolom bukti diisi saat laporan task ditulis, bukan
oleh dokumen ini.

---

## 1. Kemampuan → epic → task

| Kemampuan | Nama | Epic | Task backend | Task frontend |
| --- | --- | --- | --- | --- |
| `CAP-015` | Pemeriksaan penunjang laboratorium dan radiologi | `EPIC DOK-06` | `BE-RWI-042`, `BE-RWI-052` | `FE-RWI-049` |
| `CAP-020` | Dokumentasi SOAP | `EPIC DOK-01`, `EPIC DOK-03` | `BE-RWI-037`, `BE-RWI-043`, `BE-RWI-044`, `BE-RWI-046`, `BE-RWI-047` | `FE-RWI-045` |
| `CAP-021` | Catatan terpadu beserta verifikasi | `EPIC DOK-04` | `BE-RWI-040`, `BE-RWI-053` | `FE-RWI-046`, `FE-RWI-050` |
| `CAP-022` | Kajian medis awal | `EPIC DOK-02` | `BE-RWI-040`, `BE-RWI-045` | `FE-RWI-044` |
| `CAP-023` | Resep rawat inap dan obat pulang | `EPIC DOK-06` | `BE-RWI-042`, `BE-RWI-043`, `BE-RWI-050` | `FE-RWI-048` |
| `CAP-024` | Tindakan dokter | `EPIC DOK-06` | `BE-RWI-051` | `FE-RWI-048` |
| `CAP-025` | Pencatatan visite dokter | `EPIC DOK-05` | `BE-RWI-041`, `BE-RWI-048`, `BE-RWI-049` | `FE-RWI-047` |

**Nol kemampuan tanpa task.** Ketujuhnya punya sekurang-kurangnya satu task backend dan satu task
frontend.

### 1.1 Task fondasi yang melayani lebih dari satu kemampuan

| Task | Melayani | Kenapa tidak dipecah per kemampuan |
| --- | --- | --- |
| `BE-RWI-037` | Seluruhnya | Jalur yang diperbaiki adalah pintu masuk semua dokumentasi dokter |
| `BE-RWI-038` | `CAP-020`, `CAP-021`, `CAP-022`, `CAP-024` | Satu mekanisme koreksi untuk empat jenis dokumen; memecahnya melahirkan empat jalur koreksi |
| `BE-RWI-039` | Seluruhnya | Satu service konteks dipakai seluruh perintah klinis, **dan dipakai bersama `keperawatan`** |
| `BE-RWI-040` | `CAP-020`, `CAP-021`, `CAP-022`, `CAP-024` | Satu migration lebih aman daripada empat migration berurutan pada tabel bertetangga |
| `FE-RWI-043` | Seluruh layar dokter | Kepala konteks dan penjaga kewenangan dipakai kedelapan layar |

---

## 2. Functional requirement → task → acceptance criteria → test

| FR | Bunyi singkat | Task | Acceptance criteria | Jenis test | Status |
| --- | --- | --- | --- | --- | --- |
| `FR-DOK-001` | Catatan dibuat tanpa antrean pada perawatan berjalan | `BE-RWI-044` | `AC-CAP022-01`, `AC-CAP023-01` | Integration | ✅ Catatan dan pengkajian tersimpan tanpa antrean dan tanpa kunjungan IGD, beserta penanda perawatannya — [BE-RWI-044](../task/report/backend/BE-RWI-044.md) |
| `FR-DOK-002` | Catatan kedua pada satu kunjungan rawat inap | `BE-RWI-043`, `BE-RWI-044` | `RWI-RULE-026` aturan 4 | Integration | ✅ Terbukti lewat endpoint 4 September 2026: dua catatan berturut-turut pada satu perawatan keduanya dijawab `200` — [BE-RWI-043](../task/report/backend/BE-RWI-043.md), [BE-RWI-044](../task/report/backend/BE-RWI-044.md) |
| `FR-DOK-003` | Resep kedua sepanjang perawatan | `BE-RWI-043`, `BE-RWI-050` | `RWI-RULE-026` aturan 5 | Integration | 🟡 Aturan aplikasi dan index database sudah dilonggarkan dan diuji; pembuktian lewat endpoint menunggu jalur pemesanan resep rawat inap pada `BE-RWI-050` — [BE-RWI-043](../task/report/backend/BE-RWI-043.md) |
| `FR-DOK-004` | Perilaku rawat jalan dan MCU tidak berubah | `BE-RWI-043` | **`RWI-AC-143`** | **Regression** | ✅ Test regresi rawat jalan dan medical check-up hijau; kalimat penolakan dibandingkan **utuh** — [BE-RWI-043](../task/report/backend/BE-RWI-043.md) |
| `FR-DOK-005` | Jalur IGD tidak rusak | `BE-RWI-037`, `BE-RWI-043` | `RWI-DEC-051` | ✅ Test regresi IGD hijau pada kedua task — [BE-RWI-037](../task/report/backend/BE-RWI-037.md), [BE-RWI-043](../task/report/backend/BE-RWI-043.md) |
| `FR-DOK-006` | Kajian medis pada perawatan berjalan | `BE-RWI-045` | `AC-CAP022-01` | Integration | ✅ Kajian medis hanya lahir di atas perawatan berjalan; kunjungan tanpa perawatan ditolak `422` — [BE-RWI-045](../task/report/backend/BE-RWI-045.md) |
| `FR-DOK-007` | Kajian medis dan catatan harian berbeda record | `BE-RWI-045` | `AC-CAP022-02` | Integration | ✅ Dua record pada dua tabel; menyelesaikan salah satunya tidak menggerakkan status yang lain — [BE-RWI-045](../task/report/backend/BE-RWI-045.md) |
| `FR-DOK-008` | Catatan harian tidak menimpa kajian medis | `BE-RWI-045` | PRD `CAP-022` aturan 3 | Integration | ✅ Tiga catatan harian ditulis; isi, status, dan waktu kajian medis identik sebelum dan sesudahnya — [BE-RWI-045](../task/report/backend/BE-RWI-045.md) |
| `FR-DOK-009` | Diagnosis tersimpan terstruktur | `BE-RWI-045` | PRD `CAP-022` aturan 5 | Integration | ⛔ **Terblokir.** `TrxPatientAssessment` tidak punya kolom diagnosis kerja, dan `TrxPatientDiagnosis` mewajibkan `ConsultationId` sehingga diagnosis tidak dapat digantung pada kajian. Kamus data bagian 3 menyatakan **nol** kolom baru pada tabel itu. Menunggu keputusan struktur — [BE-RWI-045](../task/report/backend/BE-RWI-045.md) bagian 6.1 |
| `FR-DOK-010` | Koreksi kajian medis mempertahankan versi asli | `BE-RWI-038`, `BE-RWI-047` | `RWI-AC-158`, `RWI-AC-162` | Integration | Belum |
| `FR-DOK-011` | Perawat tidak dapat membuat kajian medis | `BE-RWI-045` | `VAL-DOK-05` | Integration | ✅ Pengguna yang tidak terhubung ke data dokter ditolak `403`, dan tetap boleh membuat pengkajian keperawatan — [BE-RWI-045](../task/report/backend/BE-RWI-045.md) |
| `FR-DOK-012` | Beberapa catatan sebagai lini masa | `BE-RWI-046` | `AC-CAP020-01` | Integration | ✅ `GET /doctor-consultations/episodes/{episodeId}/soap-timeline` mengembalikan catatan satu perawatan terurut waktu pemeriksaan — [BE-RWI-046](../task/report/backend/BE-RWI-046.md) |
| `FR-DOK-013` | Waktu pemeriksaan terpisah dari waktu penulisan | `BE-RWI-046` | PRD `CAP-020` aturan 2 | Integration | ✅ Urutan penulisan sengaja dibalik dari urutan pemeriksaan; lini masa mengikuti waktu pemeriksaan. Batas `VAL-DOK-13` dan `VAL-DOK-14` diuji terpisah — [BE-RWI-046](../task/report/backend/BE-RWI-046.md) |
| `FR-DOK-014` | Catatan dibuat walaupun pengkajian perawat belum selesai | `BE-RWI-044` | `AC-CAP020-02` | Integration | ✅ Pengkajian keperawatan berstatus `InProgress` tidak menahan pembuatan catatan — [BE-RWI-044](../task/report/backend/BE-RWI-044.md) |
| `FR-DOK-015` | Perawatan tertutup menolak catatan baru, menerima koreksi | `BE-RWI-047` | `AC-CAP020-03`, `RWI-AC-161` | Integration | Belum |
| `FR-DOK-016` | Koreksi tidak mengaktifkan kembali perawatan | `BE-RWI-047` | `RWI-AC-161` | Integration | Belum |
| `FR-DOK-017` | Catatan lintas profesi tampil terpisah | `BE-RWI-053` | `AC-CAP021-01` | Integration | Belum |
| `FR-DOK-018` | Verifikasi tidak mengubah penulis asli | `BE-RWI-053` | `AC-CAP021-03` | Integration | Belum |
| `FR-DOK-019` | Verifikasi hanya oleh DPJP aktif saat itu | `BE-RWI-053` | `VAL-DOK-07`, `INV-DOK-11` | Integration | Belum |
| `FR-DOK-020` | Keterlambatan terpantau tanpa menahan | `BE-RWI-053` | `AC-CAP021-02`, `VAL-DOK-25` | Integration | Belum |
| `FR-DOK-021` | Kebijakan kosong berarti nol yang menunggu | `BE-RWI-053` | `VAL-DOK-24` | Integration | Belum |
| `FR-DOK-022` | Koreksi mengembalikan ke menunggu verifikasi | `BE-RWI-053` | PRD `CAP-021` aturan 6 | Integration | Belum |
| `FR-DOK-023` | Visite tercatat beserta identitasnya | `BE-RWI-048` | `RWI-AC-153` | Integration | Belum |
| `FR-DOK-024` | Catatan tanpa event tidak menambah hitungan | `BE-RWI-048` | `RWI-AC-151` | Integration | Belum |
| `FR-DOK-025` | Visite muncul walaupun catatannya menyusul | `BE-RWI-048` | `RWI-AC-150` | Integration | Belum |
| `FR-DOK-026` | Kiriman berulang tidak melahirkan kejadian ganda | `BE-RWI-048` | `RWI-AC-152`, `RWI-AC-155` | **Integration PostgreSQL** | Belum |
| `FR-DOK-027` | Visite berdekatan diperingatkan, bukan ditolak | `BE-RWI-048` | `VAL-DOK-18` | Integration | Belum |
| `FR-DOK-028` | Hanya dokter yang dapat mencatat visite | `BE-RWI-048` | `VAL-DOK-08` | Integration | Belum |
| `FR-DOK-029` | Resep dari konteks rawat inap | `BE-RWI-050` | `AC-CAP023-01` | Integration | Belum |
| `FR-DOK-030` | Obat pulang sebagai jenis resep eksplisit | `BE-RWI-042`, `BE-RWI-050` | `AC-CAP023-03` | Integration | 🟡 Jenis resep sudah ada dan penyaringannya diuji; pengisiannya dari jalur pemesanan menunggu `BE-RWI-050` — [BE-RWI-042](../task/report/backend/BE-RWI-042.md) |
| `FR-DOK-031` | Status pemenuhan hanya dibaca | `BE-RWI-050` | `VAL-DOK-21` | Integration + **Architecture** | Belum |
| `FR-DOK-032` | Resep berulang tidak ganda | `BE-RWI-050` | `VAL-DOK-19` | Integration | Belum |
| `FR-DOK-033` | Tindakan membedakan rencana dan pelaksanaan | `BE-RWI-051` | `RWI-DOK-RQG-005` | Integration | Belum |
| `FR-DOK-034` | Kegagalan tagihan tidak menghapus catatan | `BE-RWI-051` | PRD `CAP-024` aturan 5 | Integration | Belum |
| `FR-DOK-035` | Pesanan lab membawa konteks perawatan | `BE-RWI-042`, `BE-RWI-052` | `AC-CAP015-01` | Integration | 🟡 Kolom konteks dan penyaring kunjungan sudah ada dan diuji; pengisiannya dari jalur pemesanan menunggu `BE-RWI-052` — [BE-RWI-042](../task/report/backend/BE-RWI-042.md) |
| `FR-DOK-036` | Hasil lab terbaca tanpa salinan | `BE-RWI-052` | `AC-CAP015-02` | Integration + **Architecture** | Belum |
| `FR-DOK-037` | Jalur tanpa antrean tidak gagal | `BE-RWI-037` | `DOK-TRC-DEF-01` | Integration + **Regression** | ✅ Enam test hijau, termasuk uji hitungan baris antrean sebelum dan sesudah — [BE-RWI-037](../task/report/backend/BE-RWI-037.md) |
| `FR-DOK-038` | Penanda perawatan tidak cocok ditolak | `BE-RWI-039`, `BE-RWI-044` | `VAL-DOK-26` | Integration | ✅ Terpasang pada jalur pembuatan catatan dan pengkajian, **pada kedua cabang** — berantre maupun tanpa antrean; ditolak `400` — [BE-RWI-039](../task/report/backend/BE-RWI-039.md), [BE-RWI-044](../task/report/backend/BE-RWI-044.md) |
| `FR-DOK-039` | Dua visite pada tanggal sama dihitung dua | `BE-RWI-048` | `RWI-AC-154` | Integration | Belum |
| `FR-DOK-040` | Kejadian batal tetap tersimpan, tidak dihitung | `BE-RWI-049` | `INV-DOK-08`, `VAL-DOK-28`, `VAL-DOK-29` | Integration | Belum |
| `FR-DOK-041` | Agregasi tagihan tidak mengubah kejadian klinis | `BE-RWI-049` | `RWI-AC-156` | Integration | Belum |
| `FR-DOK-042` | Pesanan radiologi membawa konteks perawatan | `BE-RWI-042`, `BE-RWI-052` | `AC-CAP015-01` | Integration | 🟡 Kolom konteks sudah ada; pengisiannya dari jalur pemesanan menunggu `BE-RWI-052` — [BE-RWI-042](../task/report/backend/BE-RWI-042.md) |
| `FR-DOK-043` | Hasil belum final diberi penanda | `BE-RWI-052` | `VAL-DOK-30` | Integration | Belum |
| `FR-DOK-044` | Finalisasi sekaligus mendaftarkan ke mesin keutuhan | `BE-RWI-038` | `RWI-AC-157` | Integration | Belum |
| `FR-DOK-045` | Koreksi pada catatan belum final ditolak | `BE-RWI-038` | `RWI-AC-159`, `VAL-DOK-32` | Integration | Belum |
| `FR-DOK-046` | Kajian medis dan tindakan ikut dapat dikoreksi | `BE-RWI-038` | `RWI-AC-162` | Integration | Belum |
| `FR-DOK-047` | Koreksi atas nama dokter berhalangan hanya DPJP aktif | `BE-RWI-047` | `RWI-AC-163`, `RWI-AC-167`, `VAL-DOK-35` | Integration | Belum |
| `FR-DOK-048` | Koreksi atas nama lain tidak mengubah penulis asli | `BE-RWI-047` | `RWI-AC-164` | Integration | Belum |

---

## 3. Decision ID → task

| Decision | Isinya | Task yang mewujudkan |
| --- | --- | --- |
| `RWI-DEC-038`, `RWI-DEC-070` | Pelonggaran antrean dan batas jumlah, terbatas rawat inap dan IGD | `BE-RWI-037`, `BE-RWI-039`, `BE-RWI-043` |
| `RWI-DEC-046` | Obat pulang sebagai jenis resep milik Farmasi | `BE-RWI-042`, `BE-RWI-050` |
| `RWI-DEC-062` | Persetujuan pemilik modul lintas modul | Prasyarat seluruh task; bukan task tersendiri |
| `RWI-DEC-081` | Nol tabel dokumentasi klinis milik Rawat Inap | Dijaga architecture test pada `BE-RWI-041` dan matriks acceptance §7 |
| `RWI-DEC-083` | Pemetaan tujuh kemampuan ke sub-modul ini | Batas scope roadmap |
| `RWI-DEC-084` | Visite adalah kejadian klinis eksplisit | `BE-RWI-041`, `BE-RWI-048`, `FE-RWI-047` |
| `RWI-DEC-085` | Setiap visite nyata satu hitungan; agregasi Billing terpisah | `BE-RWI-048`, `BE-RWI-049`, `FE-RWI-047` |
| `RWI-DEC-086` | Selesai sama dengan tertanda tangan sama dengan terkunci | `BE-RWI-038`, `FE-RWI-045` |
| `RWI-DEC-087` | Tiga jenis dokumen didaftarkan ke mesin keutuhan | `BE-RWI-038` |
| `RWI-DEC-088` | Koreksi atas nama dokter berhalangan hanya DPJP aktif | `BE-RWI-047`, `FE-RWI-045` |
| `RWI-DEC-051` | Kewajiban test regresi pada setiap perubahan mesin klinis | `BE-RWI-037`, `BE-RWI-043`, dan setiap task yang menyentuh mesin klinis |
| `RWI-RULE-021` | Batas waktu klinis — **belum final** | `BE-RWI-053` membangun mekanismenya tanpa angka |
| `RWI-RULE-026` | Tidak ada tabel tandingan dan tidak ada antrean semu | `BE-RWI-039` acceptance nomor 7; `FE-RWI-042` acceptance nomor 4 |
| `RWI-DEC-089` | `CAP-016` pemakaian alat ditunda | **Di luar scope sub-modul ini** — milik `keperawatan` |

---

## 4. Coverage gap — yang belum tercakup

Bagian ini sengaja ditulis supaya lubangnya terlihat, bukan supaya dokumen terlihat rapi.

### 4.1 Requirement yang punya task tetapi belum punya test otomatis apa pun hari ini

| Keadaan | Buktinya | Akibatnya |
| --- | --- | --- |
| ~~**Nol** test untuk konsultasi, pengkajian, catatan terpadu, tindakan, resep, dan radiologi rawat inap~~ **sebagian tertutup, diperbarui 4 September 2026** | `DOK-TRC-VER-01`; penutupan sebagian oleh `BE-RWI-037` s.d. `BE-RWI-046` | Jaring pengamannya kini **86 test** pada `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/` ditambah **19 test hak akses peran non-SuperAdmin** pada `Tests/QuilvianSystemBackend.UnitTests.InMemory/HealthServices/ClinicalManagement/`. Cakupannya: jalur tanpa antrean, service konteks klinis, bentuk kolom dan index, kejadian visite, penyaring pesanan laboratorium, **pintu masuk rawat inap**, **kajian medis**, **lini masa catatan harian**, dan **regresi poliklinik, medical check-up, serta IGD**. Yang masih kosong: catatan terpadu, tindakan, dan radiologi rawat inap. Setiap task berikutnya tetap **membawa test-nya sendiri** |
| **Nol** test yang benar-benar berjalan di atas PostgreSQL untuk sub-modul ini | Percobaan 3 September 2026 berhenti pada `BLOCKED_BY_TEST_DB_CONFIGURATION` | Dua test PostgreSQL milik `BE-RWI-041` sudah ditulis dan terkompilasi tetapi belum pernah dijalankan, dan lima migration `DOK-MVP-1` belum pernah diuji maju-mundur. Penegakan unique index dan keberhasilan migration karena itu belum terbukti pada database yang sesungguhnya dipakai |
| **Nol** test frontend untuk ruang kerja dokter dan komponen dasar klinis | `DOK-TRC-VER-01` | `FE-RWI-042` dan `FE-RWI-043` menjadi task pertama yang menulis test frontend sub-modul ini |

### 4.2 Requirement yang sengaja tidak punya task pada rilis pertama

| Requirement | Kenapa tidak punya task | Penggantinya selama MVP |
| --- | --- | --- |
| Nilai batas waktu kajian medis dan verifikasi | `RWI-RULE-021` belum disahkan; pemilik klinis belum ditunjuk | Mekanismenya dibangun `BE-RWI-053` dengan kebijakan kosong |
| Pencatatan visite atas nama dokter | Kebijakannya belum ada | Bawaan aman: hanya dokter, dijaga `VAL-DOK-08` |
| Penagihan dan agregasi tarif visite | Kebijakan milik pemilik Billing belum ada | Kejadian klinis tetap dicatat lengkap sehingga aturan apa pun dapat dijalankan mundur |
| Pembacaan balik penyerahan obat pulang | Kontrak status final Farmasi belum disetujui — `RWI-DOK-RQG-003` | Butir daftar periksa ditandai manual petugas admisi |
| Pemberitahuan otomatis | Tidak ada requirement-nya — `RWI-DOK-RQG-001` | Daftar pantau dan daftar percobaan ulang |

### 4.3 Batas yang tidak dapat dijaga mesin mana pun

| Batas | Kenapa tidak dapat dijaga | Di mana dijaganya | Test penjaganya |
| --- | --- | --- | --- |
| Dokter hanya menulis untuk pasien yang menjadi tanggung jawabnya | Mesin hak akses hanya mengenal peran terhadap endpoint | `BE-RWI-039`, `BE-RWI-044` | `VAL-DOK-06` — ⛔ **belum ditegakkan** per 4 September 2026. Service konteks sudah mampu memeriksanya, tetapi menyalakannya berarti menolak dokter konsulen dan dokter jaga yang bukan DPJP, dan kebijakan itu tidak disebut satu pun acceptance criteria `BE-RWI-044`. Menunggu keputusan pemilik |
| Kajian medis hanya oleh dokter, pengkajian keperawatan hanya oleh perawat | Keduanya berbagi satu sumber daya hak akses | `BE-RWI-045` | `VAL-DOK-05` — ✅ **ditegakkan** 4 September 2026, diturunkan dari penautan pengguna ke data dokter dan bukan dari nama peran |
| **Koreksi atas nama dokter lain hanya oleh DPJP aktif** | Penetapan berhalangan bersifat milik penulis, tidak menyebut penggantinya | `BE-RWI-047` | **`RWI-AC-167`** |
| Hasil yang dibaca milik perawatan yang sedang dibuka | Mesin hak akses tidak mengenal perawatan | `BE-RWI-052` | `VAL-DOK-31` |

> **Baris ketiga adalah yang paling mudah lolos.** Seluruh pemeriksaan hak aksesnya **berhasil**;
> yang menolak adalah aturan bisnis. Test yang hanya menguji hak akses tidak akan pernah
> menangkapnya, dan karena itu `RWI-AC-167` ditulis dengan catatan tegas.

### 4.4 Dependency lintas sub-modul yang belum punya roadmap penerima

| Butir | Keadaan | Yang harus terjadi |
| --- | --- | --- |
| Service konteks klinis bersama — `INT-DOK-01` dan `INT-KEP-01` | **Sudah dibuat `BE-RWI-039`** pada 3 September 2026 — `InpatientClinicalContextService`, terdaftar pada dependency injection | Roadmap `keperawatan` menerima **baris dependency**, bukan salinan task. Bukti: [BE-RWI-039](../task/report/backend/BE-RWI-039.md) |
| Kolom konteks pada tabel pengkajian | **Sudah dibuat `BE-RWI-040`** pada 3 September 2026 — `InpEpisodeId` dan `AssessmentType` beserta enum `PatientAssessmentType` | Roadmap `keperawatan` **memakainya apa adanya** dan tidak membuat ulang. Kolom `DueAt` dan `PolicyId` sengaja **tidak** dibuat: keduanya bergantung pada master kebijakan milik `keperawatan` yang belum ada. Bukti: [BE-RWI-040](../task/report/backend/BE-RWI-040.md) |
| Urutan daftar di dalam daftar pantau | Ditetapkan `02-module-map.md` | `FE-RWI-050` mengikuti ketetapan itu, tidak memutuskan sendiri |

---

## 5. Arah balik — task → kemampuan

| Task | Kemampuan yang dilayani |
| --- | --- |
| `BE-RWI-037` | Seluruhnya — pintu masuk dokumentasi |
| `BE-RWI-038` | `CAP-020`, `CAP-021`, `CAP-022`, `CAP-024` |
| `BE-RWI-039` | Seluruhnya |
| `BE-RWI-040` | `CAP-020`, `CAP-021`, `CAP-022`, `CAP-024` |
| `BE-RWI-041` | `CAP-025` |
| `BE-RWI-042` | `CAP-015`, `CAP-023` |
| `BE-RWI-043` | `CAP-020`, `CAP-023` |
| `BE-RWI-044` | `CAP-020`, `CAP-022` |
| `BE-RWI-045` | `CAP-022` |
| `BE-RWI-046` | `CAP-020` |
| `BE-RWI-047` | `CAP-020`, `CAP-021`, `CAP-022`, `CAP-024` |
| `BE-RWI-048` | `CAP-025` |
| `BE-RWI-049` | `CAP-025` |
| `BE-RWI-050` | `CAP-023` |
| `BE-RWI-051` | `CAP-024` |
| `BE-RWI-052` | `CAP-015` |
| `BE-RWI-053` | `CAP-021` |
| `FE-RWI-042` | Seluruhnya — keterjangkauan |
| `FE-RWI-043` | Seluruhnya — konteks pasien |
| `FE-RWI-044` | `CAP-022` |
| `FE-RWI-045` | `CAP-020` |
| `FE-RWI-046` | `CAP-021` |
| `FE-RWI-047` | `CAP-025` |
| `FE-RWI-048` | `CAP-023`, `CAP-024` |
| `FE-RWI-049` | `CAP-015` |
| `FE-RWI-050` | `CAP-021` |

---

## 6. Definition of Done tingkat sub-modul

Diturunkan dari `04-prd-to-mvp.md` bagian 19. Sub-modul dianggap selesai untuk rilis pertama hanya
bila **seluruh** butir terjawab "ya".

| No | Butir | Task pembuktinya |
| ---: | --- | --- |
| 1 | Jalur tanpa antrean tidak lagi gagal dan tidak menyentuh data antrean | `BE-RWI-037` |
| 2 | Jalur IGD dan poliklinik terbukti tidak rusak | `BE-RWI-037`, `BE-RWI-043` |
| 3 | Catatan rawat inap dapat dibuat tanpa antrean | `BE-RWI-044` ✅ |
| 4 | Catatan dan resep kedua diterima rawat inap, tetap ditolak rawat jalan | `BE-RWI-043` |
| 5 | Kajian medis dan catatan harian terbukti berbeda record | `BE-RWI-045` ✅ |
| 6 | Waktu pemeriksaan terpisah dan lini masa terurut benar | `BE-RWI-046` ✅ |
| 7 | Perawatan tertutup menolak catatan baru dan menerima koreksi | `BE-RWI-047` |
| 8 | Verifikasi tidak mengubah penulis asli | `BE-RWI-053` |
| 9 | Verifikasi hanya oleh DPJP aktif saat itu | `BE-RWI-053` |
| 10 | Catatan tanpa kejadian visite tidak menambah hitungan | `BE-RWI-048` |
| 11 | Dua visite nyata pada tanggal sama menghasilkan hitungan dua | `BE-RWI-048` |
| 12 | Kiriman ulang tetap satu kejadian, terbukti pada PostgreSQL sungguhan | `BE-RWI-048` |
| 13 | Kejadian yang dibatalkan tetap tersimpan dan tidak dihitung | `BE-RWI-049` |
| 14 | Agregasi tagihan tidak mengubah riwayat klinis | `BE-RWI-049` |
| 15 | Kegagalan penagihan tidak menghilangkan catatan tindakan | `BE-RWI-051` |
| 16 | Obat pulang terbedakan dari resep harian | `BE-RWI-050` |
| 17 | Pesanan lab dan radiologi tidak dapat dipakai lintas perawatan | `BE-RWI-052` |
| 18 | Hasil terbaca tanpa salinan, yang belum final ditandai | `BE-RWI-052` |
| 19 | Nol jalur tulis menuju status pemenuhan dan hasil penunjang | `BE-RWI-050`, `BE-RWI-052` |
| 20 | Nol tabel `Inp*` untuk dokumentasi dokter | `BE-RWI-041` |
| 21 | Nol entity baru berawalan `Trx*` | `BE-RWI-041` |
| 22 | Nol baris antrean dibuat untuk pasien rawat inap | `BE-RWI-039`, `FE-RWI-042` |
| 23 | Butir hak akses baru berfungsi bagi peran non-SuperAdmin | `BE-RWI-044` ✅ — nol butir baru diperlukan; keenam butir yang dipakai sudah ada dan terbukti dapat diberikan kepada peran non-SuperAdmin |
| 24 | Ruang kerja membaca daftar pasien dirawat, tanpa aksi antrean | `FE-RWI-042`, `FE-RWI-043` |
| 25 | Delapan layar terjangkau sesuai `IA-INP-01` dan `IA-INP-05` | `FE-RWI-042` s.d. `FE-RWI-050` |
| 26 | Kolom sensitif tidak muncul di logger | Seluruh task backend |
| 27 | Baris registry `Rad` sudah `ACTIVE` | `BE-RWI-042` |
| 28 | Finalisasi sekaligus mendaftarkan; kegagalannya membatalkan finalisasi | `BE-RWI-038` |
| 29 | Catatan final dapat dikoreksi; catatan konsep menolak koreksi | `BE-RWI-038` |
| 30 | Kajian medis dan tindakan ikut dapat dikoreksi | `BE-RWI-038` |
| 31 | Koreksi atas nama dokter lain tidak mengubah penulis aslinya | `BE-RWI-047` |
| 32 | Penetapan berhalangan tanpa masa berlaku ditolak | `BE-RWI-047` |
| 33 | Hanya DPJP aktif yang dapat mengoreksi atas nama dokter lain | `BE-RWI-047` |

**Tiga puluh tiga butir, seluruhnya punya task pembukti.** Tidak ada butir yang menggantung tanpa
pemilik pekerjaan.
